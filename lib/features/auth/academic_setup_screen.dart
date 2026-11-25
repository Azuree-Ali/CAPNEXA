import 'package:flutter/material.dart';

import 'package:capnexa/core/network/api_exception.dart';
import 'package:capnexa/core/routes/app_navigator.dart';
import 'package:capnexa/core/routes/app_routes.dart';
import 'package:capnexa/core/widgets/app_button.dart';
import 'package:capnexa/core/widgets/app_dialog.dart';
import 'package:capnexa/core/widgets/app_icon.dart';
import 'package:capnexa/core/widgets/app_text_field.dart';
import 'package:capnexa/features/auth/widgets/auth_shell.dart';
import 'package:capnexa/services/service_locator.dart';

/// بيظهر بعد Google/GitHub لو academicProfileComplete = false
class AcademicSetupScreen extends StatefulWidget {
  const AcademicSetupScreen({super.key});

  @override
  State<AcademicSetupScreen> createState() => _AcademicSetupScreenState();
}

class _AcademicSetupScreenState extends State<AcademicSetupScreen> {
  final _number = TextEditingController();
  final _dept = TextEditingController();
  final _year = TextEditingController();
  final _level = TextEditingController();
  bool _busy = false;

  @override
  void dispose() {
    for (final c in [_number, _dept, _year, _level]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _submit() async {
    if (_busy) return;
    if (_number.text.trim().isEmpty ||
        _dept.text.trim().isEmpty ||
        _year.text.trim().isEmpty) {
      _toast('Student ID, department and academic year are required.');
      return;
    }
    setState(() => _busy = true);
    try {
      await ServiceLocator.auth.completeProfile(
        studentNumber: _number.text.trim(),
        department: _dept.text.trim(),
        academicYear: _year.text.trim(),
        level: _level.text.trim().isEmpty ? null : _level.text.trim(),
      );
      if (!mounted) return;
      AppNav.go(context, Routes.home);
    } on ApiException catch (e) {
      // لو الـ profile اتكمل قبل كده منحبسش المستخدم هنا
      if (e.code == 'academic_profile_already_completed' && mounted) {
        AppNav.go(context, Routes.home);
        return;
      }
      _toast(e.message);
    } catch (_) {
      _toast('Something went wrong. Please try again.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _signOut() async {
    await ServiceLocator.auth.signOut();
    if (!mounted) return;
    AppNav.go(context, Routes.login);
  }

  void _toast(String message) {
    if (mounted) showAppSnack(context, message);
  }

  @override
  Widget build(BuildContext context) {
    return AuthShell(
      step: 'Academic details',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const AuthHeading(
            eyebrow: 'ALMOST THERE',
            title: 'Complete your profile',
            subtitle: 'We need your university details to set up your team.',
          ),
          AppTextField(label: 'Student ID', icon: AppIcons.school, controller: _number),
          const SizedBox(height: 10),
          AppTextField(label: 'Department / Major', controller: _dept),
          const SizedBox(height: 10),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(child: AppTextField(label: 'Academic Year', controller: _year)),
              const SizedBox(width: 9),
              Expanded(child: AppTextField(label: 'Level (optional)', controller: _level)),
            ],
          ),
          const SizedBox(height: 14),
          AppButton(label: 'Continue', icon: AppIcons.arrow, full: true, onPressed: _submit),
          const SizedBox(height: 6),
          AppButton(
            label: 'Sign out',
            variant: ButtonVariant.text,
            full: true,
            onPressed: _signOut,
          ),
        ],
      ),
    );
  }
}