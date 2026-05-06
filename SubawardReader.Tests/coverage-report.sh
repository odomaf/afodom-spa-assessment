#!/bin/bash

# Run tests with coverlet and display coverage report in browser.
# Usage: ./coverage-report.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

RESULTS_DIR="$SCRIPT_DIR/TestResults"
REPORT_DIR="$REPO_ROOT/TestResults/coverage-report"
TEST_PROJECT="$SCRIPT_DIR/SubawardReader.Tests.csproj"

rm -rf "$RESULTS_DIR"
mkdir -p "$RESULTS_DIR" "$REPORT_DIR"

echo "Running tests with code coverage..."
dotnet test "$TEST_PROJECT" \
  --logger "console;verbosity=minimal" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$RESULTS_DIR" \
  --settings "$SCRIPT_DIR/coverage.runsettings"

COVERAGE_FILE="$(find "$RESULTS_DIR" -type f -name "coverage.cobertura.xml" | head -n 1)"

if [[ -z "$COVERAGE_FILE" ]]; then
  echo "Coverage file not found under $RESULTS_DIR."
  exit 1
fi

# Auto-install ReportGenerator globally on first run if not already available.
echo "Generating HTML coverage report..."
if command -v reportgenerator >/dev/null 2>&1; then
  REPORT_TOOL="reportgenerator"
elif dotnet tool list -g | grep -q "dotnet-reportgenerator-globaltool"; then
  REPORT_TOOL="reportgenerator"
else
  echo "Installing ReportGenerator as global tool..."
  dotnet tool install -g dotnet-reportgenerator-globaltool
  REPORT_TOOL="reportgenerator"
fi

"$REPORT_TOOL" \
  -reports:"$COVERAGE_FILE" \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:Html

REPORT_FILE="$REPORT_DIR/index.html"
echo "Coverage report generated at: $REPORT_FILE"

# Open in Firefox first; fall back through common Windows/Git Bash paths, then system default.
OPENED_REPORT=0
WIN_REPORT_FILE="$(cygpath -w "$REPORT_FILE" 2>/dev/null || echo "$REPORT_FILE")"

if command -v firefox >/dev/null 2>&1; then
  if firefox "$REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]] && command -v firefox.exe >/dev/null 2>&1; then
  if firefox.exe "$WIN_REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]] && [[ -x "/c/Program Files/Mozilla Firefox/firefox.exe" ]]; then
  if "/c/Program Files/Mozilla Firefox/firefox.exe" "$WIN_REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]] && [[ -x "/c/Program Files (x86)/Mozilla Firefox/firefox.exe" ]]; then
  if "/c/Program Files (x86)/Mozilla Firefox/firefox.exe" "$WIN_REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if command -v cmd.exe >/dev/null 2>&1; then
  if [[ "$OPENED_REPORT" -eq 0 ]] && cmd.exe /c start "" "$WIN_REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]] && command -v powershell.exe >/dev/null 2>&1; then
  WIN_REPORT_FILE="$(cygpath -w "$REPORT_FILE" 2>/dev/null || echo "$REPORT_FILE")"
  if powershell.exe -NoProfile -Command "Start-Process '$WIN_REPORT_FILE'" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]] && command -v xdg-open >/dev/null 2>&1; then
  if xdg-open "$REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]] && command -v open >/dev/null 2>&1; then
  if open "$REPORT_FILE" >/dev/null 2>&1; then
    OPENED_REPORT=1
  fi
fi

if [[ "$OPENED_REPORT" -eq 0 ]]; then
  echo "Could not open browser automatically. Open manually: $REPORT_FILE"
fi