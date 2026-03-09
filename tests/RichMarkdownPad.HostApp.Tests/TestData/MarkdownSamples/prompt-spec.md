# Prompt Spec Sample

## Objective

Create a high-confidence implementation plan with explicit acceptance criteria.

## Acceptance Criteria Table

| ID | Requirement | Pass Condition |
| --- | --- | --- |
| AC-01 | Build succeeds | `dotnet build` exit code is 0 |
| AC-02 | Tests pass | all tests green |
| AC-03 | Logs are visible | ingestion logs include sample names |

## XML-like Payload Example

```xml
<instruction scope="editor">
  <input>markdown text</input>
  <expected>html preview</expected>
</instruction>
```

## Nested List Example

- Planning
  - Inputs
  - Outputs
- Execution
  - Build
  - Test
- Reporting
  - Findings
  - Risks

## Escaped Characters

Use literals like `<`, `>`, `&`, and backticks: `` `code` ``.
