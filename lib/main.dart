import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/app.dart';
import 'package:capnexa/services/service_locator.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
  ServiceLocator.setup(); // swap mock services for API services here
  runApp(const CapNexaApp());
}
