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
import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _first = TextEditingController();
  final _last = TextEditingController();
  final _id = TextEditingController();
  final _email = TextEditingController();
  final _dept = TextEditingController();
  final _year = TextEditingController();
  final _password = TextEditingController();
  final _confirm = TextEditingController();
  bool _busy = false;
  String? _sentTo; // بعد نجاح التسجيل
  bool _emailSent = true;

  @override
  void dispose() {
    for (final c in [
      _first,
      _last,
      _id,
      _email,
      _dept,
      _year,
      _password,
      _confirm
    ]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _register() async {
    if (_busy) return;
    if (_password.text != _confirm.text) {
      _error('Passwords do not match.');
      return;
    }
    setState(() => _busy = true);
    try {
      final result = await ServiceLocator.auth.register(RegistrationData(
        firstName: _first.text.trim(),
        lastName: _last.text.trim(),
        universityId: _id.text.trim(),
        email: _email.text.trim(),
        department: _dept.text.trim(),
        academicYear: _year.text.trim(),
        password: _password.text,
        confirmPassword: _confirm.text,
      ));
      if (!mounted) return;
      setState(() {
        _sentTo = _email.text.trim();
        _emailSent = result.emailConfirmationSent;
      });
    } on ApiException catch (e) {
      _error(e.message);
    } catch (_) {
      _error('Something went wrong. Please try again.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _resend() async {
    try {
      await ServiceLocator.auth.resendConfirmation(_sentTo!);
      if (mounted) showAppSnack(context, 'Confirmation email sent.');
    } on ApiException catch (e) {
      _error(e.message);
    } catch (_) {
      _error('Something went wrong. Please try again.');
    }
  }

  void _error(String message) {
    if (mounted) showAppSnack(context, message);
  }

  Widget _buildCheckEmail() => AuthShell(
        step: 'Student registration',
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            AuthHeading(
              eyebrow: 'ONE LAST STEP',
              title: 'Check your email',
              subtitle: _emailSent
                  ? 'We sent a confirmation link to $_sentTo. Open it, then sign in.'
                  : 'Your account was created, but the confirmation email could not be sent. Tap resend below.',
            ),
            AppButton(
              label: 'Resend confirmation email',
              variant: ButtonVariant.outline,
              full: true,
              onPressed: _resend,
            ),
            const SizedBox(height: 10),
            AppButton(
              label: 'Go to sign in',
              icon: AppIcons.arrow,
              full: true,
              onPressed: () => AppNav.replace(context, Routes.login),
            ),
          ],
        ),
      );
  @override
  Widget build(BuildContext context) {
    if (_sentTo != null) return _buildCheckEmail();
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
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                  child: AppTextField(
                      label: 'First name',
                      icon: AppIcons.user,
                      controller: _first)),
              const SizedBox(width: 9),
              Expanded(
                  child: AppTextField(label: 'Last name', controller: _last)),
            ],
          ),
          const SizedBox(height: 10),
          AppTextField(
              label: 'University ID', icon: AppIcons.school, controller: _id),
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
              Expanded(
                  child: AppTextField(
                      label: 'Department / Major', controller: _dept)),
              const SizedBox(width: 9),
              Expanded(
                  child:
                      AppTextField(label: 'Academic Year', controller: _year)),
            ],
          ),
          const SizedBox(height: 10),
          AppTextField(
              label: 'Password',
              icon: AppIcons.lock,
              controller: _password,
              obscure: true),
          const SizedBox(height: 10),
          Row(children: [
            rule('8+ characters'),
            const SizedBox(width: 14),
            rule('Number & symbol')
          ]),
          const SizedBox(height: 10),
          AppTextField(
              label: 'Confirm password',
              icon: AppIcons.lock,
              controller: _confirm,
              obscure: true),
          const SizedBox(height: 10),
          AppButton(
              label: 'Create account',
              icon: AppIcons.arrow,
              full: true,
              onPressed: _register),
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
