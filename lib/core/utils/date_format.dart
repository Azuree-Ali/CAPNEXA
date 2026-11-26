const _months = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
  'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
];

String monthShort(DateTime d) => _months[d.month - 1];

String formatDate(DateTime d) => '${monthShort(d)} ${d.day}, ${d.year}';