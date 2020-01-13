module.exports = {
  root: true,
  env: {
    node: true
  },
  extends: [
    "eslint:recommended",
    "plugin:@typescript-eslint/recommended",
    "plugin:@typescript-eslint/eslint-recommended",
    "plugin:prettier/recommended",
    "plugin:vue/recommended",
    "prettier",
    "prettier/@typescript-eslint",
    "@vue/prettier",
    "@vue/typescript"
  ],
  plugins: ["@typescript-eslint", "prettier", "vue"],
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "error" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off",
    "require-atomic-updates": "off",
    "@typescript-eslint/indent": ["error", 2],
    "@typescript-eslint/explicit-function-return-type": "off",
    "@typescript-eslint/no-explicit-any": "off", // 出来れば対応したい
    "@typescript-eslint/explicit-member-accessibility": ["error", { accessibility: "no-public" }]
  },
  parserOptions: {
    parser: "@typescript-eslint/parser",
    // 遅い
    // project: './tsconfig.json',
    ecmaVersion: 2020,
    sourceType: "module"
  }
};
