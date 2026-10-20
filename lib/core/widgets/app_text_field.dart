import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';
import 'package:capnexa/core/widgets/app_icon.dart';

/// `.field` – label + bordered input (single line or textarea).
class AppTextField extends StatefulWidget {
  const AppTextField({
    super.key,
    required this.label,
    this.hint,
    this.icon,
    this.initialValue,
    this.controller,
    this.obscure = false,
    this.textarea = false,
    this.error,
    this.keyboardType,
    this.onChanged,
    this.inputFormatters,
  });

  final String label;
  final String? hint;
  final AppIcons? icon;
  final String? initialValue;
  final TextEditingController? controller;
  final bool obscure;
  final bool textarea;
  final String? error;
  final TextInputType? keyboardType;
  final ValueChanged<String>? onChanged;
  final List<TextInputFormatter>? inputFormatters;

  @override
  State<AppTextField> createState() => _AppTextFieldState();
}

class _AppTextFieldState extends State<AppTextField> {
  late final FocusNode _focus = FocusNode()..addListener(() => setState(() {}));
  TextEditingController? _own;
  late bool _hidden = widget.obscure;

  TextEditingController get _controller =>
      widget.controller ?? (_own ??= TextEditingController(text: widget.initialValue));

  @override
  void dispose() {
    _focus.dispose();
    _own?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final hasError = widget.error != null;
    final focused = _focus.hasFocus;
    final borderColor = hasError
        ? AppColors.red700
        : focused
            ? AppColors.blue600
            : AppColors.fieldBorder;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(widget.label, style: AppText.s(10, w: FontWeight.w700, color: AppColors.navy900)),
        const SizedBox(height: 6),
        AnimatedContainer(
          duration: const Duration(milliseconds: 120),
          constraints: const BoxConstraints(minHeight: 42),
          padding: const EdgeInsets.symmetric(horizontal: 11),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: borderColor),
            boxShadow: focused && !hasError
                ? const [BoxShadow(color: AppColors.blue100, spreadRadius: 3)]
                : null,
          ),
          child: Row(
            crossAxisAlignment: widget.textarea ? CrossAxisAlignment.start : CrossAxisAlignment.center,
            children: [
              if (widget.icon != null) ...[
                Padding(
                  padding: EdgeInsets.only(top: widget.textarea ? 12 : 0),
                  child: AppIcon(widget.icon!, size: 18, color: AppColors.iconMuted),
                ),
                const SizedBox(width: 8),
              ],
              Expanded(
                child: TextField(
                  controller: _controller,
                  focusNode: _focus,
                  obscureText: _hidden,
                  minLines: widget.textarea ? 4 : 1,
                  maxLines: widget.textarea ? 6 : 1,
                  keyboardType: widget.textarea ? TextInputType.multiline : widget.keyboardType,
                  inputFormatters: widget.inputFormatters,
                  onChanged: widget.onChanged,
                  cursorColor: AppColors.blue600,
                  style: AppText.s(10.5, color: AppColors.ink, height: widget.textarea ? 1.5 : null),
                  decoration: InputDecoration(
                    isDense: true,
                    border: InputBorder.none,
                    hintText: widget.hint,
                    hintStyle: AppText.s(10.5, color: AppColors.hint),
                    contentPadding: EdgeInsets.symmetric(vertical: widget.textarea ? 10 : 12),
                  ),
                ),
              ),
              if (widget.obscure)
                GestureDetector(
                  onTap: () => setState(() => _hidden = !_hidden),
                  behavior: HitTestBehavior.opaque,
                  child: Padding(
                    padding: const EdgeInsets.only(left: 8),
                    child: AppIcon(AppIcons.eye, size: 18, color: AppColors.iconMuted),
                  ),
                ),
            ],
          ),
        ),
        if (hasError) ...[
          const SizedBox(height: 6),
          Text(widget.error!, style: AppText.s(8.5, color: AppColors.red700)),
        ],
      ],
    );
  }
}
