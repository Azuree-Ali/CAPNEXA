export interface LoginContent {
  administratorSupervisorAccess: string;
  welcomeBack: string;
  signInWithYourUniversityCredentialsToOversee: string;
  openDemonstrationWorkspace: string;
  separator: string;
  rememberMe: string;
  forgotPassword: string;
  signingIn: string;
  signIn: string;
  aboutCAPNEXA: string;
  yourCapstoneJourneyConnected: string;
  manageCapstoneWorkFromProposalToEvaluation: string;
  features: readonly {
    icon: "hub" | "fact_check" | "verified_user";
    title: string;
    text: string;
  }[];
  legalLinks: readonly string[];
  showPassword: string;
  hidePassword: string;
  password: string;
  separator2: string;
}
export const loginCopy: LoginContent = {
  administratorSupervisorAccess: "Administrator & Supervisor access",
  welcomeBack: "Welcome back",
  signInWithYourUniversityCredentialsToOversee:
    "Sign in with your university credentials to oversee capstone projects.",
  openDemonstrationWorkspace: "Open demonstration workspace",
  separator: "•",
  rememberMe: "Remember me",
  forgotPassword: "Forgot password?",
  signingIn: "Signing in...",
  signIn: "Sign In",
  aboutCAPNEXA: "About CAPNEXA",
  yourCapstoneJourneyConnected: "Your Capstone Journey, Connected",
  manageCapstoneWorkFromProposalToEvaluation: "Manage capstone work from proposal to evaluation.",
  features: [
    {
      icon: "hub",
      title: "Centralized Management",
      text: "Keep project work in one place.",
    },
    {
      icon: "fact_check",
      title: "AI-Assisted Academic Integrity",
      text: "AI assistance for academic integrity workflows.",
    },
    {
      icon: "verified_user",
      title: "Secure Collaboration",
      text: "Collaborate securely across the capstone journey.",
    },
  ],
  legalLinks: ["Privacy Policy", "Terms of Service", "Help & Support"],
  showPassword: "Show password",
  hidePassword: "Hide password",
  password: "Password",
  separator2: "••••••••••••",
};
