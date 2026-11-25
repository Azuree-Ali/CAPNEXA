import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/network/api_endpoints.dart';
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/network/token_storage.dart';

class ApiClient {
  ApiClient({required this.tokenStorage, this.onSessionExpired})
      : dio = Dio(_options()) {
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        if (!_isAuthPath(options.path)) {
          final token = await tokenStorage.readToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
        }
        handler.next(options);
      },
      onError: (error, handler) async {
        final request = error.requestOptions;
        final is401 = error.response?.statusCode == 401;
        if (!is401 ||
            _isAuthPath(request.path) ||
            request.extra['retried'] == true) {
          return handler.next(error);
        }
        final refreshed = await _refreshTokens();
        if (!refreshed) return handler.next(error);
        try {
          request.extra['retried'] = true;
          final response = await dio.fetch<dynamic>(request);
          handler.resolve(response);
        } on DioException catch (e) {
          handler.next(e);
        }
      },
    ));

    if (kDebugMode) {
      // مفيش headers ولا bodies عشان الـ token والباسورد ميتطبعوش
      dio.interceptors.add(LogInterceptor(
        requestHeader: false,
        requestBody: false,
        responseHeader: false,
        responseBody: false,
      ));
    }
  }

  final Dio dio;
  final TokenStorage tokenStorage;

  /// بيتنادى لما الـ refresh token يتأكد إنه منتهي (نوجّه المستخدم لـ login)
  final void Function()? onSessionExpired;

  late final Dio _refreshDio = Dio(_options());
  Future<bool>? _refreshing;

  static BaseOptions _options() => BaseOptions(
        baseUrl: AppConstants.apiBaseUrl,
        connectTimeout: const Duration(seconds: 5),
        receiveTimeout: const Duration(seconds: 20),
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
        },
      );

  bool _isAuthPath(String path) =>
      path.toLowerCase().contains('/identity/auth/');

  Future<bool> _refreshTokens() =>
      _refreshing ??= _doRefresh().whenComplete(() => _refreshing = null);

  Future<bool> _doRefresh() async {
    final refresh = await tokenStorage.readRefreshToken();
    if (refresh == null) return false;
    try {
      final res = await _refreshDio.post<dynamic>(
        ApiEndpoints.refresh,
        data: {'refreshToken': refresh},
      );
      final data = res.data;
      if (data is! Map) return false;
      final access = data['accessToken']?.toString();
      final newRefresh = data['refreshToken']?.toString();
      if (access == null || newRefresh == null) return false;
      await tokenStorage.saveTokens(
          accessToken: access, refreshToken: newRefresh);
      return true;
    } catch (e) {
      if (e is DioException && e.response?.statusCode == 401) {
        await tokenStorage.clear();
        onSessionExpired?.call();
      }
      return false;
    }
  }

  Future<dynamic> get(String path, {Map<String, dynamic>? query}) =>
      _send(() => dio.get<dynamic>(path, queryParameters: query));

  Future<dynamic> post(String path, {Object? body}) =>
      _send(() => dio.post<dynamic>(path, data: body));

  Future<dynamic> put(String path, {Object? body}) =>
      _send(() => dio.put<dynamic>(path, data: body));

  Future<dynamic> delete(String path) => _send(() => dio.delete<dynamic>(path));

  Future<dynamic> _send(Future<Response<dynamic>> Function() call) async {
    try {
      return (await call()).data;
    } on DioException catch (e) {
      throw _toApiException(e);
    }
  }

  ApiException _toApiException(DioException e) {
    final status = e.response?.statusCode;
    final data = e.response?.data;

    final noConnection = e.type == DioExceptionType.connectionTimeout ||
        e.type == DioExceptionType.sendTimeout ||
        e.type == DioExceptionType.receiveTimeout ||
        e.type == DioExceptionType.connectionError;
    if (noConnection) {
      return const ApiException('Check your connection and try again.');
    }

    String? message;
    String? code;
    if (data is Map) {
      code = data['code']?.toString();
      message =
          (data['message'] ?? data['detail'] ?? data['title'])?.toString();
      // أخطاء الـ validation: ناخد أول رسالة محددة
      final errors = data['errors'];
      if (errors is Map && errors.isNotEmpty) {
        final first = errors.values.first;
        if (first is List && first.isNotEmpty) message = first.first.toString();
      }
    }
    return ApiException(
      message ?? 'Something went wrong (${status ?? 'unknown'}).',
      statusCode: status,
      code: code,
    );
  }
}
