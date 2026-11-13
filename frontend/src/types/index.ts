export type TroubleRequest = { deviceCategory: string; deviceName: string; brand: string; model: string; problemDescription: string }
export type FieldErrors = Record<string, string[]>
export type DiagnosticResult = { problemSummary: string; possibleCauses: {cause: string; likelihood: string; explanation: string}[]; diagnosticSteps: string[]; toolsAndMaterials: string[]; difficulty: string; safetyWarnings: string[]; whenToContactProfessional: string }
