# Parsing Refactor Plan (2026-05-07)

## Summary Recommendations

- Extract recipient row detection logic into a helper method to reduce duplication and clarify intent.
- Add a TryGetValueOrDefault extension method for dictionary value retrieval with a default, to simplify output logic.
- Only extract subheading/amount extraction helpers or worksheet navigation utilities if repeated code or future reuse is anticipated; avoid unnecessary abstraction otherwise.
- Current code is maintainable, but these two changes are clear wins for readability and maintainability.

## Next Steps

- Implement recipient row helper and TryGetValueOrDefault extension.
- Review for further refactoring only if more duplication appears.
