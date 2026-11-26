import 'package:flutter/material.dart';

import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_scaffold.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/core/widgets/async_scaffold.dart';
import 'package:capnexa/models/student.dart';
import 'package:capnexa/services/service_locator.dart';

/// بيشغّل عملية async ويعرض أي خطأ كـ snack. بيرجّع true لو نجحت.
Future<bool> _attempt(BuildContext context, Future<void> Function() action) async {
  try {
    await action();
    return true;
  } on ApiException catch (e) {
    if (context.mounted) showAppSnack(context, e.message);
  } catch (_) {
    if (context.mounted) showAppSnack(context, 'Something went wrong. Please try again.');
  }
  return false;
}

// ---------------------------------------------------------------- Edit profile

class EditProfileScreen extends StatelessWidget {
  const EditProfileScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AsyncScaffold<Student>(
      title: 'Edit profile',
      subtitle: 'Update your name',
      tab: AppTab.profile,
      loader: ServiceLocator.auth.currentStudent,
      builder: (context, s) => _EditProfileForm(student: s),
    );
  }
}

class _EditProfileForm extends StatefulWidget {
  const _EditProfileForm({required this.student});

  final Student student;

  @override
  State<_EditProfileForm> createState() => _EditProfileFormState();
}

class _EditProfileFormState extends State<_EditProfileForm> {
  late final _first = TextEditingController(
      text: widget.student.givenName.isNotEmpty
          ? widget.student.givenName
          : widget.student.firstName);
  late final _last = TextEditingController(text: widget.student.familyName);
  bool _busy = false;

  @override
  void dispose() {
    _first.dispose();
    _last.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (_busy) return;
    if (_first.text.trim().isEmpty || _last.text.trim().isEmpty) {
      showAppSnack(context, 'First and last name are required.');
      return;
    }
    setState(() => _busy = true);
    final ok = await _attempt(
      context,
      () => ServiceLocator.auth
          .updateProfile(firstName: _first.text.trim(), lastName: _last.text.trim()),
    );
    if (!mounted) return;
    setState(() => _busy = false);
    if (ok) {
      showAppSnack(context, 'Profile updated');
      AppNav.go(context, Routes.profile); // يعيد تحميل الـ Profile بالبيانات الجديدة
    }
  }

  @override
  Widget build(BuildContext context) {
    return ScreenBody(
      children: [
        AppTextField(label: 'First name', icon: AppIcons.user, controller: _first),
        AppTextField(label: 'Last name', controller: _last),
        Text(
          'Your student ID, department and academic year can’t be changed here.',
          style: AppText.s(8.5, color: AppColors.muted, height: 1.5),
        ),
        AppButton(label: 'Save changes', full: true, onPressed: _save),
      ],
    );
  }
}

// ------------------------------------------------------------- Change password

class ChangePasswordScreen extends StatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  State<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  final _current = TextEditingController();
  final _new = TextEditingController();
  final _confirm = TextEditingController();
  bool _busy = false;

  @override
  void dispose() {
    for (final c in [_current, _new, _confirm]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _save() async {
    if (_busy) return;
    if (_new.text != _confirm.text) {
      showAppSnack(context, 'Passwords do not match.');
      return;
    }
    setState(() => _busy = true);
    final ok = await _attempt(
      context,
      () => ServiceLocator.auth.changePassword(
        currentPassword: _current.text,
        newPassword: _new.text,
        confirmPassword: _confirm.text,
      ),
    );
    if (!mounted) return;
    setState(() => _busy = false);
    if (ok) {
      // الباك اند بيلغي الـ refresh tokens بعد تغيير الباسورد
      await ServiceLocator.auth.signOut();
      if (!mounted) return;
      showAppSnack(context, 'Password changed. Please sign in again.');
      AppNav.go(context, Routes.login);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Change password',
      subtitle: 'Keep your account secure',
      tab: AppTab.profile,
      body: ScreenBody(
        children: [
          AppTextField(
              label: 'Current password', icon: AppIcons.lock, controller: _current, obscure: true),
          Text(
            'Signed in with Google or GitHub and never set a password? Leave this empty.',
            style: AppText.s(8.5, color: AppColors.muted, height: 1.5),
          ),
          AppTextField(
              label: 'New password', icon: AppIcons.lock, controller: _new, obscure: true),
          AppTextField(
              label: 'Confirm new password',
              icon: AppIcons.lock,
              controller: _confirm,
              obscure: true),
          AppButton(label: 'Change password', full: true, onPressed: _save),
        ],
      ),
    );
  }
}

// ---------------------------------------------------------------- Change email

class ChangeEmailScreen extends StatefulWidget {
  const ChangeEmailScreen({super.key});

  @override
  State<ChangeEmailScreen> createState() => _ChangeEmailScreenState();
}

class _ChangeEmailScreenState extends State<ChangeEmailScreen> {
  final _email = TextEditingController();
  bool _busy = false;

  @override
  void dispose() {
    _email.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (_busy) return;
    final email = _email.text.trim();
    if (email.isEmpty) {
      showAppSnack(context, 'Enter the new email address.');
      return;
    }
    setState(() => _busy = true);
    final ok = await _attempt(context, () => ServiceLocator.auth.requestEmailChange(email));
    if (!mounted) return;
    setState(() => _busy = false);
    if (ok) {
      showAppSnack(context, 'We sent a confirmation link to $email. Your email changes after you confirm it.');
      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Change email',
      subtitle: 'Confirm a new email address',
      tab: AppTab.profile,
      body: ScreenBody(
        children: [
          AppTextField(
            label: 'New email',
            icon: AppIcons.mail,
            controller: _email,
            keyboardType: TextInputType.emailAddress,
          ),
          Text(
            'Your email stays the same until you open the link we send to the new address.',
            style: AppText.s(8.5, color: AppColors.muted, height: 1.5),
          ),
          AppButton(label: 'Send confirmation link', full: true, onPressed: _save),
        ],
      ),
    );
  }
}