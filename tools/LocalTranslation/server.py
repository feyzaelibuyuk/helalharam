"""Local LibreTranslate launcher: Windows Unicode paths and private text logging."""
import ctypes
import logging
import os
from pathlib import Path

# SentencePiece's native Windows file reader needs an ASCII-compatible path.
packages = Path.home() / ".local/share/argos-translate/packages"
packages.mkdir(parents=True, exist_ok=True)
if os.name == "nt":
    buffer = ctypes.create_unicode_buffer(32768)
    if ctypes.windll.kernel32.GetShortPathNameW(str(packages), buffer, len(buffer)):
        os.environ["ARGOS_PACKAGES_DIR"] = buffer.value
from libretranslate.main import main
# Argos INFO messages contain complete source text. Never log user text.
logging.getLogger("argostranslate.utils").disabled = True
if __name__ == "__main__":
    main()
