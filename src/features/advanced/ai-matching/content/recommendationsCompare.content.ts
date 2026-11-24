export interface ComparisonContent {
  title: string;
  description: string;
  parent: { label: string; route: string };
  field: string;
  expertise: string;
  availability: string;
  evidence: string;
}
export const recommendationsCompareContent: ComparisonContent = {
  title: "Compare supervisors",
  description: "Compare experience, capacity, and evidence side by side.",
  parent: { label: "Supervisor recommendations", route: "/admin/recommendations" },
  field: "Criterion",
  expertise: "Expertise",
  availability: "Available slots",
  evidence: "Supporting evidence",
};
