# Agent Deploy Rules — task9-api

## ⚠️ DEPLOY PROTOCOL — BẮT BUỘC khi user nói "deploy"

GitHub Actions CHỈ trigger khi commit message có prefix `[API]`.
`--no-ff --no-edit` KHÔNG trigger CI vì merge commit KHÔNG có `[API]`.

### Quy trình deploy API (BẮT BUỘC theo thứ tự):

```bash
# 1. Merge vào test (staging)
git checkout test && git pull origin test
git merge --squash <feature-branch>
git commit -m "[API] <type>(<scope>): <mô tả>"
git push origin test
# → CI build Docker image tag "test" (staging)

# 2. Merge vào main (production) — CHỈ sau khi test OK
git checkout main && git pull origin main
git merge --squash <feature-branch>
git commit -m "[API] <type>(<scope>): <mô tả>"
git push origin main
# → CI build Docker image tag "net9" (production)
```

### Trigger rules GitHub Actions:
| Prefix commit | Branch | Kết quả |
|---|---|---|
| `[API]` | `test` | Build image tag `test` → staging |
| `[API]` | `main` | Build image tag `net9` → production |
| ❌ Không có prefix | bất kỳ | **KHÔNG build, KHÔNG deploy** |

### SAI — KHÔNG DÙNG:
```bash
# ❌ SAI: merge commit không có [API] prefix → CI không trigger
git merge --no-ff --no-edit <branch>
```

### Sau deploy xong — update submodule ở workspace root:
```bash
cd /path/to/task9-workspace
git add services/api
git commit -m "chore: update api submodule — <mô tả ngắn>"
git push
```
