# Saves And Yandex Games

## Ownership

- `CloudSaveService` owns save lifecycle.
- Saves live in `YG2.saves`.
- Keep legacy save migration near save logic.
- Use stable config ids through `SaveKeyUtility`.

## Rules

- Services may read/write `YG2.saves` today.
- Current code still has direct `YG2.saves` writes in services; do not expand this casually for save-heavy new work.
- For substantial save-heavy changes, prefer a focused repository/helper around feature saves.
- Keep service code focused on gameplay state.
