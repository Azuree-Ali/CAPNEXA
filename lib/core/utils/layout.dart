import 'package:flutter/widgets.dart';

/// Inserts [gap] between every child (CSS `display:flex; gap`).
List<Widget> spaced(List<Widget> children, double gap, {Axis axis = Axis.vertical}) {
  final out = <Widget>[];
  for (var i = 0; i < children.length; i++) {
    if (i > 0) {
      out.add(axis == Axis.vertical ? SizedBox(height: gap) : SizedBox(width: gap));
    }
    out.add(children[i]);
  }
  return out;
}
