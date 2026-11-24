export interface RecommendationsContent {
  title: string;
  description: string;
  candidateLabel: string;
  capacity: string;
  evidence: string;
  compare: string;
  nothing: string;
  comparisonRoute: string;
  candidates: readonly {
    id: string;
    name: string;
    expertise: string;
    free: number;
    evidence: string;
  }[];
}
export const recommendationsContent: RecommendationsContent = {
  title: "Supervisor recommendations",
  description: "Review documented suitability and current capacity for Campus Maps.",
  candidateLabel: "Select for comparison",
  capacity: "Available slots",
  evidence: "Matching evidence",
  compare: "Compare selected candidates",
  nothing: "Select two candidates to compare.",
  comparisonRoute: "/admin/recommendations/compare",
  candidates: [
    {
      id: "olivia",
      name: "Dr. Olivia Bennett",
      expertise: "Distributed systems",
      free: 2,
      evidence: "Has experience with distributed data ingestion and accessibility reviews.",
    },
    {
      id: "sam",
      name: "Dr. Sam Reed",
      expertise: "Geospatial computing",
      free: 1,
      evidence: "Has advised projects involving mapping and geospatial query design.",
    },
  ],
};
