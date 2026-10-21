import 'package:flutter/material.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/features/auth/widgets/auth_shell.dart';
import 'package:capnexa/services/auth_service.dart';
import 'package:capnexa/services/service_locator.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _email = TextEditingController(text: 'lina.hassan@northbridge.edu');
  final _password = TextEditingController(text: 'Passw0rd!!');
  bool _remember = true;
  bool _busy = false;
  SocialProvider? _connecting;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _signIn() async {
    if (_busy) return;
    setState(() => _busy = true);
    await ServiceLocator.auth.signIn(email: _email.text.trim(), password: _password.text);
    if (!mounted) return;
    AppNav.go(context, Routes.home);
  }

  Future<void> _social(SocialProvider provider) async {
    if (_connecting != null) return;
    setState(() => _connecting = provider);
    await ServiceLocator.auth.signInWithProvider(provider);
    if (!mounted) return;
    AppNav.go(context, Routes.home);
  }

  String _name(SocialProvider p) => switch (p) {
        SocialProvider.google => 'Google',
        SocialProvider.github => 'GitHub',
      };

  Provider _icon(SocialProvider p) => switch (p) {
        SocialProvider.google => Provider.google,
        SocialProvider.github => Provider.github,
      };

  @override
  Widget build(BuildContext context) {
    return AuthShell(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          AuthHeading(
            eyebrow: 'WELCOME BACK',
            title: 'Sign in to ${AppConstants.brandName}',
            subtitle: 'Continue managing your graduation project.',
          ),
          AppTextField(
            label: 'University email',
            icon: AppIcons.mail,
            controller: _email,
            keyboardType: TextInputType.emailAddress,
          ),
          const SizedBox(height: 14),
          AppTextField(
            label: 'Password',
            icon: AppIcons.lock,
            controller: _password,
            obscure: true,
          ),
          const SizedBox(height: 14),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              GestureDetector(
                onTap: () => setState(() => _remember = !_remember),
                behavior: HitTestBehavior.opaque,
                child: Row(
                  children: [
                    Container(
                      width: 14,
                      height: 14,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: _remember ? AppColors.blue700 : Colors.white,
                        borderRadius: BorderRadius.circular(4),
                        border: _remember ? null : Border.all(color: AppColors.fieldBorder),
                      ),
                      child: _remember
                          ? const AppIcon(AppIcons.check, size: 10, color: Colors.white)
                          : null,
                    ),
                    const SizedBox(width: 7),
                    Text('Remember me', style: AppText.s(9.5, color: AppColors.muted)),
                  ],
                ),
              ),
              GestureDetector(
                onTap: () => showAppSnack(context, 'Password reset will be available with the API.'),
                child: Text('Forgot password?',
                    style: AppText.s(9.5, w: FontWeight.w800, color: AppColors.blue700)),
              ),
            ],
          ),
          const SizedBox(height: 14),
          AppButton(label: 'Sign in securely', icon: AppIcons.arrow, full: true, onPressed: _signIn),
          const SizedBox(height: 14),
          const _OrDivider(),
          const SizedBox(height: 14),
          for (final p in SocialProvider.values) ...[
            _ProviderButton(
              label: _connecting == p ? 'Connecting to ${_name(p)}…' : 'Sign in with ${_name(p)}',
              provider: _icon(p),
              connecting: _connecting == p,
              disabled: _connecting != null && _connecting != p,
              onTap: () => _social(p),
            ),
            if (p != SocialProvider.values.last) const SizedBox(height: 8),
          ],
          AuthSwitch(
            prompt: 'New to ${AppConstants.brandName}?',
            action: 'Create account',
            onTap: () => AppNav.replace(context, Routes.register),
          ),
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.only(top: 15),
            decoration: const BoxDecoration(border: Border(top: BorderSide(color: AppColors.line))),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const AppIcon(AppIcons.shield, size: 16, color: AppColors.iconMuted),
                const SizedBox(width: 6),
                Text('Protected by your university identity',
                    style: AppText.s(9, color: AppColors.iconMuted)),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _OrDivider extends StatelessWidget {
  const _OrDivider();

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        const Expanded(child: Divider(height: 1, color: AppColors.line)),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 11),
          child: Text('OR CONTINUE WITH',
              style: AppText.s(8, w: FontWeight.w700, color: const Color(0xFF909BAC), ls: .25)),
        ),
        const Expanded(child: Divider(height: 1, color: AppColors.line)),
      ],
    );
  }
}

class _ProviderButton extends StatelessWidget {
  const _ProviderButton({
    required this.label,
    required this.provider,
    required this.connecting,
    required this.disabled,
    required this.onTap,
  });

  final String label;
  final Provider provider;
  final bool connecting;
  final bool disabled;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final radius = BorderRadius.circular(11);
    return Opacity(
      opacity: disabled ? .58 : 1,
      child: Material(
        color: connecting ? AppColors.blue50 : Colors.white,
        shape: RoundedRectangleBorder(
          borderRadius: radius,
          side: BorderSide(color: connecting ? const Color(0xFFACBEEB) : AppColors.fieldBorder),
        ),
        child: InkWell(
          borderRadius: radius,
          onTap: disabled ? null : onTap,
          child: Container(
            constraints: const BoxConstraints(minHeight: 42),
            padding: const EdgeInsets.symmetric(horizontal: 12),
            child: Row(
              children: [
                Container(
                  width: 28,
                  height: 28,
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    color: const Color(0xFFF5F7FA),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: connecting
                      ? const SizedBox(
                          width: 14,
                          height: 14,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: AppColors.blue700,
                            backgroundColor: Color(0xFFCBD7F5),
                          ),
                        )
                      : ProviderIcon(provider),
                ),
                const SizedBox(width: 9),
                Expanded(
                  child: Text(
                    label,
                    style: AppText.s(9.5,
                        w: FontWeight.w700,
                        color: connecting ? AppColors.blue700 : AppColors.navy900),
                  ),
                ),
                const AppIcon(AppIcons.arrow, size: 15, color: Color(0xFF9AA5B5)),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
