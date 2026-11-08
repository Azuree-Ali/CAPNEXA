import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/network/token_storage.dart';

class ApiClient {
  ApiClient({required this.tokenStorage})
      : dio = Dio(BaseOptions(
          baseUrl: AppConstants.apiBaseUrl,
          connectTimeout: const Duration(seconds: 8),
          receiveTimeout: const Duration(seconds: 20),
          headers: {'Accept': 'application/json'},
        )) {
    // يضيف الـ token تلقائيًا على كل request
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await tokenStorage.readToken();
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    ));
    if (kDebugMode) {
      dio.interceptors.add(LogInterceptor(requestBody: true, responseBody: true));
    }
  }

  final Dio dio;
  final TokenStorage tokenStorage;

  Future<dynamic> get(String path, {Map<String, dynamic>? query}) =>
      _send(() => dio.get<dynamic>(path, queryParameters: query));

  Future<dynamic> post(String path, {Object? body}) =>
      _send(() => dio.post<dynamic>(path, data: body));

  Future<dynamic> put(String path, {Object? body}) =>
      _send(() => dio.put<dynamic>(path, data: body));

  Future<dynamic> delete(String path) =>
      _send(() => dio.delete<dynamic>(path));

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
    if (data is Map) {
      message = (data['detail'] ?? data['title'] ?? data['message'])?.toString();
    }
    return ApiException(
      message ?? 'Something went wrong (${status ?? 'unknown'}).',
      statusCode: status,
    );
  }
}