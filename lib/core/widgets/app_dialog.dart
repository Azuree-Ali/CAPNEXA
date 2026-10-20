import 'package:flutter/material.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_button.dart';

/// Reusable confirmation dialog styled with the design tokens
/// (white surface, 16px radius, navy title, muted copy, app buttons).
Future<bool> showAppDialog(
  BuildContext context, {
  required String title,
  required String message,
  String confirmLabel = 'Confirm',
  String? cancelLabel = 'Cancel',
  bool destructive = false,
  Widget? content,
}) async {
  final result = await showDialog<bool>(
    context: context,
    barrierColor: const Color.fromRGBO(4, 14, 30, .5),
    builder: (ctx) => Dialog(
      backgroundColor: Colors.white,
      surfaceTintColor: Colors.transparent,
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: const BorderSide(color: AppColors.line),
      ),
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(title, style: AppText.s(15, w: FontWeight.w700, color: AppColors.navy900, ls: -.3)),
            const SizedBox(height: 6),
            Text(message, style: AppText.p),
            if (content != null) ...[const SizedBox(height: 12), content],
            const SizedBox(height: 16),
            Row(
              children: [
                if (cancelLabel != null) ...[
                  Expanded(
                    child: AppButton(
                      label: cancelLabel,
                      variant: ButtonVariant.outline,
                      onPressed: () => Navigator.of(ctx).pop(false),
                    ),
                  ),
                  const SizedBox(width: 8),
                ],
                Expanded(
                  child: AppButton(
                    label: confirmLabel,
                    variant: destructive ? ButtonVariant.danger : ButtonVariant.primary,
                    onPressed: () => Navigator.of(ctx).pop(true),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    ),
  );
  return result ?? false;
}

/// Floating snackbar styled like the rest of the app.
void showAppSnack(BuildContext context, String message) {
  final messenger = ScaffoldMessenger.of(context);
  messenger
    ..hideCurrentSnackBar()
    ..showSnackBar(
      SnackBar(
        behavior: SnackBarBehavior.floating,
        backgroundColor: AppColors.navy900,
        margin: const EdgeInsets.fromLTRB(17, 0, 17, 12),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        duration: const Duration(seconds: 2),
        content: Text(message, style: AppText.s(10, w: FontWeight.w600, color: Colors.white)),
      ),
    );
}
