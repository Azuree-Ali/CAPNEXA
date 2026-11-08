import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class TokenStorage {
  TokenStorage([FlutterSecureStorage? storage])
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;
  static const _accessKey = 'access_token';

  Future<String?> readToken() => _storage.read(key: _accessKey);
  Future<void> saveToken(String token) =>
      _storage.write(key: _accessKey, value: token);
  Future<void> clear() => _storage.delete(key: _accessKey);
}