import { exampleStudent, type StudentRecord } from "../content/studentProfile.content";
let records: StudentRecord[] = [exampleStudent];
const listeners = new Set<() => void>();
export function subscribe(fn: () => void) {
  listeners.add(fn);
  return () => {
    listeners.delete(fn);
  };
}
export const snapshot = () => records;
export function save(student: StudentRecord) {
  records = records.some((r) => r.id === student.id)
    ? records.map((r) => (r.id === student.id ? student : r))
    : [...records, student];
  listeners.forEach((fn) => fn());
}
