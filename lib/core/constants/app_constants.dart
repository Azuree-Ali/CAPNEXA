/// App-wide constants.
class AppConstants {
  AppConstants._();

  /// Brand name shown in the UI. The Figma export is branded "GradFlow";
  /// change this single value to switch every occurrence.
  static const brandName = 'CapNexa';
  static const appVersion = '1.0.0';

  /// Placeholder for the future ASP.NET Core Web API.
  static const apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5000/api',
  );

  /// Simulated network latency used by the mock services.
  static const mockLatency = Duration(milliseconds: 350);
}

class AppLayout {
  AppLayout._();

  /// Content is centred and capped on tablets / foldables.
  static const maxContentWidth = 480.0;
  static const screenHPadding = 17.0;
}
