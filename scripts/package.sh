#!/usr/bin/env bash
set -euo pipefail

if [ $# -lt 1 ]; then
  echo "Usage: $0 <version>" >&2
  exit 1
fi

VERSION="$1"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT_DIR="$ROOT_DIR/.package"
PUBLISH_DIR="$OUT_DIR/publish"
DIST_DIR="$OUT_DIR/dist"

rm -rf "$OUT_DIR"
mkdir -p "$DIST_DIR"

dotnet publish \
  "$ROOT_DIR/Jellyfin.Plugin.MediaRetentionGuardian/MediaRetentionGuardian.csproj" \
  -c Release \
  --no-self-contained \
  -o "$PUBLISH_DIR"

TIMESTAMP="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
sed \
  -e "s/{{PLUGIN_VERSION}}/$VERSION/g" \
  -e "s/{{BUILD_TIMESTAMP}}/$TIMESTAMP/g" \
  "$ROOT_DIR/meta.json" > "$DIST_DIR/meta.json"

cp "$PUBLISH_DIR/MediaRetentionGuardian.dll" "$DIST_DIR/"
if [ -f "$PUBLISH_DIR/MediaRetentionGuardian.pdb" ]; then
  cp "$PUBLISH_DIR/MediaRetentionGuardian.pdb" "$DIST_DIR/"
fi
cp "$PUBLISH_DIR/thumb.png" "$DIST_DIR/"
if [ -f "$PUBLISH_DIR/logo.svg" ]; then
  cp "$PUBLISH_DIR/logo.svg" "$DIST_DIR/"
fi
cp -R "$PUBLISH_DIR/Resources" "$DIST_DIR/"

(
  cd "$DIST_DIR"
  zip -r "$ROOT_DIR/MediaRetentionGuardian_v${VERSION}.zip" ./*
)

echo "Package built: $ROOT_DIR/MediaRetentionGuardian_v${VERSION}.zip"
