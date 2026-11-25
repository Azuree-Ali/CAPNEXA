export interface SupervisorPickerContent {
  label: string;
  choose: string;
  search: string;
  filter: string;
  loading: string;
  empty: string;
  error: string;
  retry: string;
  previous: string;
  next: string;
  slots: string;
}
export const supervisorPickerContent: SupervisorPickerContent = {
  label: "Available supervisor",
  choose: "Choose a supervisor",
  search: "Find a supervisor",
  filter: "Find",
  loading: "Loading supervisors with available capacity…",
  empty: "No available supervisors match this search.",
  error: "Unable to load available supervisors.",
  retry: "Retry",
  previous: "Previous",
  next: "Next",
  slots: "available slots",
};
