#!/usr/bin/env bash
# generar-contexto.sh
# Genera "contexto-para-otro-chat.txt" en la raíz del repositorio con el estado del
# proyecto, para compartirlo con otro chat de IA.
#
# Uso (Git Bash, desde cualquier carpeta del repositorio):
#   bash scripts/generar-contexto.sh                   cambios de la rama actual respecto a su base
#   bash scripts/generar-contexto.sh --todo            además, todos los archivos relevantes del proyecto
#   bash scripts/generar-contexto.sh --base <ref>      fuerza la base (rama, etiqueta o commit)
#   bash scripts/generar-contexto.sh --salida <ruta>   cambia el archivo de salida
#
# Nunca modifica el repositorio. Solo lee y escribe el archivo de salida.

set -u
set -o pipefail

BASE=""
TODO=0
SALIDA=""
MAX_BYTES=200000
DOC="docs/contexto-proyecto.md"

while [ $# -gt 0 ]; do
  case "$1" in
    --base)
      [ $# -ge 2 ] || { echo "Falta el valor de --base" >&2; exit 1; }
      BASE="$2"; shift 2 ;;
    --salida)
      [ $# -ge 2 ] || { echo "Falta el valor de --salida" >&2; exit 1; }
      SALIDA="$2"; shift 2 ;;
    --todo)
      TODO=1; shift ;;
    -h|--help)
      sed -n '2,12p' "$0"; exit 0 ;;
    *)
      echo "Argumento desconocido: $1" >&2; exit 1 ;;
  esac
done

RAIZ="$(git rev-parse --show-toplevel 2>/dev/null)" || {
  echo "No estás dentro de un repositorio Git." >&2; exit 1; }
cd "$RAIZ" || exit 1

git rev-parse --verify --quiet HEAD >/dev/null || {
  echo "El repositorio no tiene ningún commit todavía." >&2; exit 1; }

SALIDA="${SALIDA:-contexto-para-otro-chat.txt}"
RAMA="$(git rev-parse --abbrev-ref HEAD)"
HEAD_SHA="$(git rev-parse --short HEAD)"

# ---------------------------------------------------------------- rama base
if [ -z "$BASE" ]; then
  for candidato in origin/HEAD main master; do
    if git rev-parse --verify --quiet "$candidato" >/dev/null; then
      BASE="$candidato"; break
    fi
  done
fi

PUNTO_BASE=""
NOTA_BASE=""
if [ -n "$BASE" ]; then
  PUNTO_BASE="$(git merge-base HEAD "$BASE" 2>/dev/null || true)"
fi
if [ -z "$PUNTO_BASE" ]; then
  PUNTO_BASE="$(git rev-list --max-parents=0 HEAD | tail -n 1)"
  NOTA_BASE="no se pudo calcular la base; se usa el primer commit del repositorio"
  BASE="${BASE:-(ninguna)}"
fi
if [ -z "$NOTA_BASE" ] && [ "$(git rev-parse HEAD)" = "$PUNTO_BASE" ]; then
  NOTA_BASE="la rama actual coincide con su base: solo se listan cambios sin confirmar (usa --base <ref> o --todo para ver más)"
fi

# ---------------------------------------------------------------- reglas
# Rutas que nunca se incluyen: binarios, generados, secretos y ruido.
excluir_ruta() {
  case "$1" in
    bin/*|*/bin/*|obj/*|*/obj/*|.vs/*|*/.vs/*|.git/*|node_modules/*|*/node_modules/*) return 0 ;;
    tools/*.exe|*.exe|*.dll|*.pdb|*.nupkg|*.zip) return 0 ;;
    *.png|*.jpg|*.jpeg|*.gif|*.ico|*.webp|*.svg|*.woff|*.woff2|*.ttf) return 0 ;;
    *.snk|*.pfx|*.p12|*.pem|*.key|*.cer|*.crt) return 0 ;;
    .env|.env.*|*/.env|*/.env.*|*secrets.json|*.user|*.suo|*.local.json) return 0 ;;
    */Migrations/*.Designer.cs|*/Migrations/*ModelSnapshot.cs) return 0 ;;
    */wwwroot/css/tailwind.css) return 0 ;;
    contexto-para-otro-chat.txt|*/contexto-para-otro-chat.txt) return 0 ;;
    scripts/generar-contexto.sh) return 0 ;;
    package-lock.json|*.lock) return 0 ;;
  esac
  return 1
}

# Solo se vuelcan archivos de texto del proyecto.
extension_relevante() {
  case "$1" in
    *.cs|*.csproj|*.slnx|*.sln|*.props|*.targets|*.razor|*.cshtml|*.json|*.css|*.html|*.md|*.txt|*.sh|*.sql|*.xml|*.yml|*.yaml|*.config) return 0 ;;
    .editorconfig|.gitignore|.gitattributes) return 0 ;;
  esac
  return 1
}

# Sustituye valores con forma de secreto por una marca. Es una red de seguridad:
# no reemplaza la revisión humana del archivo final.
redactar() {
  sed -E \
    -e 's/(^|[^[:alnum:]_])((Password|Pwd)=)[^;[:space:]"]+/\1\2***REDACTADO***/Ig' \
    -e 's/("(password|pwd|secret|clientsecret|client_secret|token|accesstoken|apikey|api_key)"[[:space:]]*:[[:space:]]*")[^"]+(")/\1***REDACTADO***\3/Ig' \
    -e 's/(Bearer[[:space:]]+)[A-Za-z0-9._~+\/=-]{16,}/\1***REDACTADO***/Ig' \
    -e 's/(ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9]{20,}/***REDACTADO***/g' \
    -e 's/github_pat_[A-Za-z0-9_]{20,}/***REDACTADO***/g' \
    -e 's/AKIA[0-9A-Z]{16}/***REDACTADO***/g' \
    -e 's/(sk|pk)-[A-Za-z0-9]{20,}/***REDACTADO***/g'
}

# ---------------------------------------------------------------- lista de archivos
if [ "$TODO" -eq 1 ]; then
  MODO="proyecto completo (todos los archivos relevantes)"
  LISTA="$(git -c core.quotepath=false ls-files --cached --others --exclude-standard | sort -u)"
else
  MODO="cambios respecto a la base (confirmados y sin confirmar)"
  LISTA="$( {
      git -c core.quotepath=false diff --name-only --diff-filter=ACMRT "$PUNTO_BASE" --
      git -c core.quotepath=false ls-files --others --exclude-standard
    } | sort -u )"
fi
BORRADOS="$(git -c core.quotepath=false diff --name-only --diff-filter=D "$PUNTO_BASE" -- | sort -u)"

TMP="$(mktemp)"
trap 'rm -f "$TMP"' EXIT
OMITIDOS=()
INCLUIDOS=0

{
  echo "CONTEXTO PARA OTRO CHAT - ITZYOLOTL"
  echo "Generado:         $(date -Iseconds)"
  echo "Repositorio:      $(basename "$RAIZ")"
  echo "Rama actual:      $RAMA  (HEAD $HEAD_SHA)"
  echo "Base de la rama:  $BASE  (punto de bifurcación $(git rev-parse --short "$PUNTO_BASE"))"
  [ -n "$NOTA_BASE" ] && echo "Nota sobre la base: $NOTA_BASE"
  echo "Modo:             $MODO"
  echo "Los valores con forma de secreto se sustituyen por ***REDACTADO***. Revisa el archivo antes de compartirlo."

  echo
  echo "==================== 1. CONTEXTO DEL PROYECTO ===================="
  if [ -f "$DOC" ]; then
    redactar < "$DOC" | tr -d '\r'
  else
    echo "(AVISO: no existe $DOC; el otro chat no tendrá arquitectura, plan ni instrucciones.)"
  fi

  echo
  echo "==================== 2. ENTORNO ===================="
  echo "git:      $(git --version 2>/dev/null)"
  echo "dotnet:   $(dotnet --list-sdks 2>/dev/null | tr '\n' ';')"
  echo "docker:   $(docker --version 2>/dev/null)"
  if [ -x tools/tailwindcss.exe ]; then
    echo "tailwind: $(tools/tailwindcss.exe --help 2>&1 | head -n 1)"
  fi

  echo
  echo "==================== 3. GIT ===================="
  echo "--- Estado (git status --short --branch)"
  git status --short --branch
  echo
  echo "--- Últimos 20 commits (todas las ramas locales)"
  git log --oneline --decorate --all -n 20
  echo
  echo "--- Commits de la rama actual desde la base"
  if [ "$(git rev-parse HEAD)" = "$PUNTO_BASE" ]; then
    echo "(ninguno: la rama coincide con su base)"
  else
    git log --oneline --decorate "$PUNTO_BASE"..HEAD
  fi
  echo
  echo "--- Resumen de cambios respecto a la base (git diff --stat)"
  git -c core.quotepath=false diff --stat "$PUNTO_BASE" --
  echo
  echo "--- Archivos nuevos sin seguimiento"
  git -c core.quotepath=false ls-files --others --exclude-standard
  echo
  echo "--- Archivos borrados respecto a la base"
  if [ -n "$BORRADOS" ]; then echo "$BORRADOS"; else echo "(ninguno)"; fi

  echo
  echo "==================== 4. ARCHIVOS ===================="
  while IFS= read -r f; do
    [ -z "$f" ] && continue
    [ "$f" = "$DOC" ] && continue
    [ -f "$f" ] || continue

    if excluir_ruta "$f"; then
      OMITIDOS+=("$f - excluido por regla (binario, generado, secreto o ruido)"); continue
    fi
    if ! extension_relevante "$f"; then
      OMITIDOS+=("$f - extensión no relevante"); continue
    fi
    if ! grep -Iq . "$f" 2>/dev/null; then
      OMITIDOS+=("$f - binario o vacío"); continue
    fi
    tam="$(wc -c < "$f" | tr -d ' ')"
    if [ "$tam" -gt "$MAX_BYTES" ]; then
      OMITIDOS+=("$f - demasiado grande ($tam bytes)"); continue
    fi
    if grep -Eq -- '-----BEGIN [A-Z ]*PRIVATE KEY-----' "$f"; then
      OMITIDOS+=("$f - contiene una clave privada"); continue
    fi

    INCLUIDOS=$((INCLUIDOS + 1))
    echo
    echo "----- ARCHIVO: $f ($tam bytes) -----"
    redactar < "$f" | tr -d '\r'
    echo
    echo "----- FIN: $f -----"
  done <<< "$LISTA"

  echo
  echo "==================== 5. ARCHIVOS OMITIDOS ===================="
  if [ "${#OMITIDOS[@]}" -eq 0 ]; then
    echo "(ninguno)"
  else
    printf '%s\n' "${OMITIDOS[@]}"
  fi
  echo
  echo "Archivos incluidos: $INCLUIDOS"
} > "$TMP"

# ---------------------------------------------------------------- aviso de seguridad
SOSPECHOSAS="$(grep -nEi '(password|pwd|secret|token|api[_-]?key|private key|bearer)[^[:alnum:]]{0,3}[:=]' "$TMP" \
  | grep -v 'REDACTADO' | cut -d: -f1 | head -n 25 | tr '\n' ' ')"

mv "$TMP" "$SALIDA"
trap - EXIT

echo "Generado: $SALIDA  ($(wc -c < "$SALIDA" | tr -d ' ') bytes, $INCLUIDOS archivos incluidos, ${#OMITIDOS[@]} omitidos)"
echo "Rama: $RAMA   Base: $BASE"
[ -n "$NOTA_BASE" ] && echo "Nota: $NOTA_BASE"

if [ -n "$SOSPECHOSAS" ]; then
  echo
  echo "REVISA estas líneas de $SALIDA antes de compartirlo (pueden contener datos sensibles):"
  echo "  $SOSPECHOSAS"
fi

if ! git check-ignore -q "$SALIDA"; then
  echo
  echo "AVISO: $SALIDA no está en .gitignore. Agrégalo para no subirlo por accidente a Git."
fi