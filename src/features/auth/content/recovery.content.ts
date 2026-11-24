export interface RecoveryContent {
  separator: string;
  facultyPortal: string;
  resetYourPassword: string;
  enterTheEmailAddressAssociatedWithYourAccount: string;
  backToSignIn: string;
  previewNotice: string;
  liveUnavailable: string;
  sendingRequest: string;
  sendResetInstructions: string;
}
export const recoveryCopy: RecoveryContent = {
  separator: "|",
  facultyPortal: "Faculty Portal",
  resetYourPassword: "Reset your password",
  enterTheEmailAddressAssociatedWithYourAccount:
    "Enter the email address associated with your account.",
  backToSignIn: "Back to sign-in",
  previewNotice: "This is a demonstration. No recovery email will be sent.",
  liveUnavailable:
    "Online password recovery is unavailable. Contact your university administrator.",
  sendingRequest: "Sending request...",
  sendResetInstructions: "Simulate Recovery Request",
};
