import 'package:flutter/material.dart';

import 'package:capnexa/core/constants/app_constants.dart';
import 'package:capnexa/core/theme/app_colors.dart';
import 'package:capnexa/core/theme/app_text.dart';

/// `.logo` – three-bar mark + wordmark.
class BrandLogo extends StatelessWidget {
  const BrandLogo({super.key, this.inverse = false, this.compact = false});

  final bool inverse;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    Widget bar(double h) => Container(
          width: 4,
          height: h,
          decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(2)),
        );
    final mark = Container(
      width: 27,
      height: 27,
      padding: const EdgeInsets.all(5),
      decoration: BoxDecoration(color: AppColors.blue600, borderRadius: BorderRadius.circular(8)),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [bar(7), const SizedBox(width: 2), bar(14), const SizedBox(width: 2), bar(10)],
      ),
    );
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        mark,
        if (!compact) ...[
          const SizedBox(width: 10),
          Text(
            AppConstants.brandName,
            style: AppText.s(19,
                w: FontWeight.w800, ls: -.5, color: inverse ? Colors.white : AppColors.navy900),
          ),
        ],
      ],
    );
  }
}
