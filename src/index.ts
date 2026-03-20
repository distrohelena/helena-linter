import { createRecommendedConfig } from "./configs/recommended.js";
import { helenaRules } from "./rules/index.js";
import type { HelenaLinterPlugin } from "./types/plugin-export.js";

/**
 * Provides the default Helena ESLint plugin export.
 */
const helenaLinter = {
  meta: {
    name: "@distrohelena/linter",
  },
  rules: helenaRules,
  configs: {
    recommended: [] as HelenaLinterPlugin["configs"]["recommended"],
  },
} satisfies HelenaLinterPlugin;

helenaLinter.configs.recommended = createRecommendedConfig(helenaLinter);

export default helenaLinter;
