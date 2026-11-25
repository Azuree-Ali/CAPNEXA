import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/features/auth/widgets/auth_shell.dart';
import 'package:capnexa/services/service_locator.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final _email = TextEditingController();
  final _otp = TextEditingController();
  final _password = TextEditingController();
  final _confirm = TextEditingController();
  bool _busy = false;
  bool _otpSent = false;

  @override
  void dispose() {
    for (final c in [_email, _otp, _password, _confirm]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _sendOtp() async {
    final email = _email.text.trim();
    if (email.isEmpty) {
      _toast('Enter your email first.');
      return;
    }
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await ServiceLocator.auth.requestPasswordReset(email);
      if (!mounted) return;
      setState(() => _otpSent = true);
      _toast('If this email has an account, a 6-digit code was sent.');
    } on ApiException catch (e) {
      _toast(e.message);
    } catch (_) {
      _toast('Something went wrong. Please try again.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _reset() async {
    final otp = _otp.text.trim();
    if (otp.length != 6) {
      _toast('Enter the 6-digit code.');
      return;
    }
    if (_password.text != _confirm.text) {
      _toast('Passwords do not match.');
      return;
    }
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await ServiceLocator.auth.resetPassword(
        email: _email.text.trim(),
        otp: otp,
        newPassword: _password.text,
        confirmPassword: _confirm.text,
      );
      if (!mounted) return;
      showAppSnack(context, 'Password reset. Please sign in.');
      AppNav.go(context, Routes.login);
    } on ApiException catch (e) {
      _toast(e.message);
    } catch (_) {
      _toast('Something went wrong. Please try again.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _toast(String message) {
    if (mounted) showAppSnack(context, message);
  }

  @override
  Widget build(BuildContext context) {
    return AuthShell(
      step: 'Account recovery',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: _otpSent ? _resetStage() : _emailStage(),
      ),
    );
  }

  List<Widget> _emailStage() => [
        const AuthHeading(
          eyebrow: 'ACCOUNT RECOVERY',
          title: 'Forgot password?',
          subtitle: 'Enter your university email and we’ll send you a 6-digit code.',
        ),
        AppTextField(
          label: 'University email',
          icon: AppIcons.mail,
          controller: _email,
          keyboardType: TextInputType.emailAddress,
        ),
        const SizedBox(height: 14),
        AppButton(label: 'Send code', icon: AppIcons.arrow, full: true, onPressed: _sendOtp),
        AuthSwitch(
          prompt: 'Remembered it?',
          action: 'Sign in',
          onTap: () => AppNav.replace(context, Routes.login),
        ),
      ];

  List<Widget> _resetStage() => [
        AuthHeading(
          eyebrow: 'CHECK YOUR EMAIL',
          title: 'Enter the code',
          subtitle: 'We sent a 6-digit code to ${_email.text.trim()}. It is valid for 10 minutes.',
        ),
        AppTextField(
          label: '6-digit code',
          icon: AppIcons.lock,
          controller: _otp,
          keyboardType: TextInputType.number,
          inputFormatters: [
            FilteringTextInputFormatter.digitsOnly,
            LengthLimitingTextInputFormatter(6),
          ],
        ),
        const SizedBox(height: 10),
        AppTextField(
          label: 'New password',
          icon: AppIcons.lock,
          controller: _password,
          obscure: true,
        ),
        const SizedBox(height: 10),
        AppTextField(
          label: 'Confirm new password',
          icon: AppIcons.lock,
          controller: _confirm,
          obscure: true,
        ),
        const SizedBox(height: 14),
        AppButton(label: 'Reset password', icon: AppIcons.arrow, full: true, onPressed: _reset),
        const SizedBox(height: 6),
        AppButton(
          label: 'Resend code',
          variant: ButtonVariant.text,
          full: true,
          onPressed: _sendOtp,
        ),
        AppButton(
          label: 'Use a different email',
          variant: ButtonVariant.text,
          full: true,
          onPressed: () => setState(() => _otpSent = false),
        ),
      ];
}