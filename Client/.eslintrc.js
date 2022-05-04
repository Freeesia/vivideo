module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    "eslint:recommended",
    "plugin:@typescript-eslint/recommended",
    "plugin:@typescript-eslint/eslint-recommended",
    "plugin:prettier/recommended",
    "plugin:vue/recommended",
    "prettier",
    "@vue/prettier",
    "@vue/typescript",
  ],
  plugins: ["@typescript-eslint", "prettier", "vue"],
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "error" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
    "require-atomic-updates": "off",
    "comma-dangle": [
      "warn",
      {
        arrays: "always-multiline",
        objects: "always-multiline",
        imports: "always-multiline",
        exports: "always-multiline",
        functions: "only-multiline",
      },
    ],
    "@typescript-eslint/indent": ["error", 2],
    "@typescript-eslint/explicit-function-return-type": "off",
    "@typescript-eslint/explicit-module-boundary-types": "off",
    "@typescript-eslint/no-explicit-any": "off", // 出来れば対応したい
    "@typescript-eslint/no-empty-function": "off",
    "@typescript-eslint/no-empty-interface": "off",
    "vue/no-reserved-props": [
      "error",
      {
        vueVersion: 2,
      },
    ],
  },
  parserOptions: {
    parser: "@typescript-eslint/parser",
    // 遅い
    // project: './tsconfig.json',
    ecmaVersion: 2020,
    sourceType: "module",
  },
  ignorePatterns: ["src/components.d.ts"],
};
