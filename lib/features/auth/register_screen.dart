import 'package:flutter/material.dart';

import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/features/auth/widgets/auth_shell.dart';
import 'package:capnexa/models/student.dart';
import 'package:capnexa/services/service_locator.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _name = TextEditingController(text: 'Lina Hassan');
  final _id = TextEditingController(text: '2021-04812');
  final _email = TextEditingController(text: 'lina.hassan@northbridge.edu');
  final _dept = TextEditingController(text: 'Computer Science');
  final _year = TextEditingController(text: 'Year 4');
  final _password = TextEditingController(text: 'Passw0rd!!');
  final _confirm = TextEditingController(text: 'Passw0rd!!');
  bool _busy = false;

  @override
  void dispose() {
    for (final c in [_name, _id, _email, _dept, _year, _password, _confirm]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _register() async {
    if (_busy) return;
    setState(() => _busy = true);
    await ServiceLocator.auth.register(RegistrationData(
      fullName: _name.text.trim(),
      universityId: _id.text.trim(),
      email: _email.text.trim(),
      department: _dept.text.trim(),
      academicYear: _year.text.trim(),
      password: _password.text,
    ));
    if (!mounted) return;
    AppNav.go(context, Routes.home);
  }

  @override
  Widget build(BuildContext context) {
    Widget rule(String text) => Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const AppIcon(AppIcons.check, size: 13, color: AppColors.teal700),
            const SizedBox(width: 4),
            Text(text, style: AppText.s(8, color: AppColors.teal700)),
          ],
        );
    return AuthShell(
      step: 'Student registration',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const AuthHeading(
            eyebrow: 'CREATE YOUR ACCOUNT',
            title: 'Let’s get you started',
            subtitle: 'Use your official university information.',
          ),
          AppTextField(label: 'Full name', icon: AppIcons.user, controller: _name),
          const SizedBox(height: 10),
          AppTextField(label: 'University ID', icon: AppIcons.school, controller: _id),
          const SizedBox(height: 10),
          AppTextField(
              label: 'University email',
              icon: AppIcons.mail,
              controller: _email,
              keyboardType: TextInputType.emailAddress),
          const SizedBox(height: 10),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(child: AppTextField(label: 'Department / Major', controller: _dept)),
              const SizedBox(width: 9),
              Expanded(child: AppTextField(label: 'Academic Year', controller: _year)),
            ],
          ),
          const SizedBox(height: 10),
          AppTextField(label: 'Password', icon: AppIcons.lock, controller: _password, obscure: true),
          const SizedBox(height: 10),
          Row(children: [rule('8+ characters'), const SizedBox(width: 14), rule('Number & symbol')]),
          const SizedBox(height: 10),
          AppTextField(
              label: 'Confirm password', icon: AppIcons.lock, controller: _confirm, obscure: true),
          const SizedBox(height: 10),
          AppButton(label: 'Create account', icon: AppIcons.arrow, full: true, onPressed: _register),
          AuthSwitch(
            prompt: 'Already registered?',
            action: 'Sign in',
            onTap: () => AppNav.replace(context, Routes.login),
          ),
        ],
      ),
    );
  }
}
