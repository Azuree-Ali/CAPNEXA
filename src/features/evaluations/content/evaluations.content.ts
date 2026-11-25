export interface EvaluationsContent {
  title: string;
  description: string;
  overall: string;
  percentage: string;
  projectLabel: string;
  columns: readonly string[];
  project: string;
  assessments: readonly { criterion: string; score: number; maximum: number; feedback: string }[];
}
export const evaluationsContent: EvaluationsContent = {
  title: "Evaluation results",
  description: "View scores and feedback recorded by supervisors in the demonstration cohort.",
  overall: "Total score",
  percentage: "Score percentage",
  projectLabel: "Project",
  columns: ["Criterion", "Score", "Evaluator feedback"],
  project: "Campus Atlas",
  assessments: [
    {
      criterion: "Design rationale",
      score: 18,
      maximum: 20,
      feedback: "Accessibility requirements are supported by clear user scenarios.",
    },
    {
      criterion: "Implementation plan",
      score: 16,
      maximum: 20,
      feedback: "Add acceptance criteria for the route editor.",
    },
  ],
};
