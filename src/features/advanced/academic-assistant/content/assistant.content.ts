export interface AssistantContent {
  title: string;
  description: string;
  question: string;
  ask: string;
  suggestions: string;
  noAnswer: string;

  answerHeading: string;
  ambiguous: string;
  entries: readonly {
    question: string;
    terms: readonly string[];
    answer: string;
    source: { label: string; route: string };
  }[];
}
export const assistantContent: AssistantContent = {
  title: "Academic assistant",
  description:
    "Search the demonstration knowledge base for answers supported by a workspace source.",
  question: "Academic question",
  ask: "Find answer",
  suggestions: "Example questions",
  noAnswer: "No verified answer is available for this query.",
  ambiguous: "More than one source matches. Choose the question you meant.",
  answerHeading: "Answer from the knowledge base",
  entries: [
    {
      question: "When is the design specification due?",
      terms: ["design specification", "design deadline"],
      answer: "Campus Atlas has its design specification scheduled for 15 December.",
      source: { label: "Atlas milestone schedule", route: "/admin/milestones/timeline" },
    },
    {
      question: "What feedback was given on the design brief?",
      terms: ["design brief", "wheelchair route criteria"],
      answer:
        "The first review asked the team to clarify wheelchair route criteria and document accessibility checks for each building.",
      source: { label: "Design brief review history", route: "/admin/reviews/atlas-design" },
    },
  ],
};
