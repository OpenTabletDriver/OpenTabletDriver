[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | Ελληνικά | [Magyar](README_HU.md)

Το OpenTabletDriver είναι ένα open source, cross platform, user mode driver για γραφίδες. Ο στόχος του OpenTabletDriver είναι το να είναι οσο πιο cross platform γίνετε με την περρισότερη προσβασιμότιτα σε ένα εύκολο και γρήγορο gui.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Υποστιριζόμενες Γραφίδες

Όλες οι καταστάσης τον γραφίδον που είναι υποστιριζόμενες, μη δοκιμασμένες, και προγραμματισμένες μπορούν να βρεθούν εδώ. Μπορείτε να βρείτε συνήθεις λύσεις προβλημάτων στο wiki για την πλατφόρμα σας.

- [Υποστιριζόμενες Γραφίδες](https://opentabletdriver.net/Tablets)

# Εγκατάσταση

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Τρέχοντας τα binaries του OpenTabletDriver

Το OpenTabletDriver λειτουργεί ως δύο ξεχωριστές διαδικασίες που αλληλεπιδρούν μεταξύ τους απρόσκοπτα. Το ενεργό πρόγραμμα που κάνει όλο το χειρισμό δεδομένων της γραφίδας είναι το `OpenTabletDriver.Daemon`, ενώ το frontend GUI είναι το `OpenTabletDriver.UX.*`, όπου το `*` εξαρτάται από την πλατφόρμα σας <sup>1</sup>. Ο δαίμονας πρέπει να ξεκινήσει για να λειτουργήσει οτιδήποτε, ωστόσο το GUI είναι περιττό. Εάν έχετε υπάρχουσες ρυθμίσεις, θα πρέπει να ισχύουν κατά την εκκίνηση του δαίμονα.

> <sup>1</sup> Τα Windows Χρησιμοποιούν `Wpf`, Ta Linux Χρησιμοποιούν `Gtk`, και τα MacOS Χρησιμοποιούν `MacOS`. Αυτό ως επί το πλείστον μπορεί να αγνοηθεί εάν δεν το δημιουργήσετε από την πηγή, καθώς θα παρέχεται μόνο η σωστή έκδοση.

## Κατασκευή του OpenTabletDriver από τον πηγαίο κώδικα

Οι απαιτήσεις για τη δημιουργία του OpenTabletDriver είναι συνεπείς σε όλες τις πλατφόρμες. Η εκτέλεση του OpenTabletDriver σε κάθε πλατφόρμα απαιτεί διαφορετικές εξαρτήσεις.

### Όλες η πλατφόρμες

- .NET 6 SDK (μπορεί να το κατεβάσετε απο [εδώ](https://dotnet.microsoft.com/download/dotnet/6.0) - Θέλετε το SDK για την πλατφόρμα σας, οι χρήστες Linux θα πρέπει να εγκαταστήσουν μέσω διαχειριστή πακέτων όπου είναι δυνατόν)

#### Windows

Τρέξτε το `build.ps1` για να κατασκευάσετε το binary επίσης το binary βγένη στο 'bin' φάκελο. Αυτές οι εκδόσεις θα εκτελούνται σε φορητή λειτουργία από προεπιλογή.

#### Linux

Απαιτούμενα πακέτα (ορισμένα πακέτα μπορεί να είναι προεγκατεστημένα για τη διανομή σας):

- libx11
- libxrandr
- libevdev2
- GTK+3

Τρέξτε το `./eng/linux/package.sh`. Άμα η "package" κατασκευή επιθημήτε, Υπάρχει επίσημη υποστήριξη για τις ακόλουθες μορφές συσκευασίας:

| Μορφή πακέτου | Command |
| --- | ---|
| Generic binary tarball (`.tar.gz`) | `./eng/linux/package.sh --package BinaryTarBall` |
| Debian package (`.deb`) | `./eng/linux/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/linux/package.sh --package RPM` |

Το Generic binary tarball είναι designed να είναι αποσιμπιεσμενο στον φάκελο root.

#### MacOS [Πειραματικό]

Τρέξτε το `./eng/macos/package.sh --package true`

# Δυνατότητες

- Πλήρως εγγενές GUI πλατφόρμας
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Πλήρες εργαλείο κονσόλας 
  - Γρήγορη απόκτηση, αλλαγή, φόρτωση ή αποθήκευση ρυθμίσεων
  - Υποστήριξη δέσμης ενεργειών (έξοδος json) 
- Απόλυτη τοποθέτηση δρομέα 
  - Περιοχή οθόνης και περιοχή tablet
  - Κεντρικές αγκυρωμένες μετατοπίσεις
  - Ακριβής περιστροφή περιοχής
- Σχετική τοποθέτηση δρομέα 
  - PX/mm οριζόντια και κατακόρυφη ευαισθησία 
- Δέστρες στυλό 
  - Άκρη με δέστρες πίεσης
  - Δέστρες κλειδιών Express
  - Συνδέσεις κουμπιών στυλό 
  - Συνδέσεις κουμπιών ποντικιού
  - Συνδέσεις πληκτρολογίου
  - Εξωτερικές συνδέσεις plugin
- Αποθήκευση και φόρτωση ρυθμίσεων
  - Φορτώνει αυτόματα τις ρυθμίσεις χρήστη μέσω του `settings.json` στον ριζικό κατάλογο των ρυθμίσεων ενεργού χρήστη `%localappdata%` ή `.config`.
- Επεξεργαστής διαμόρφωσης
  - Σας επιτρέπει να δημιουργήσετε, να τροποποιήσετε και να διαγράψετε διαμορφώσεις.
  - Δημιουργήστε διαμορφώσεις από ορατές συσκευές HID 
- Πρόσθετα 
  - Φίλτρα
  - Τρόποι εξόδου 
  - Εργαλεία

# Συνεισφορά στο OpenTabletDriver

Αν θέλετε να συνεισφέρετε στο OpenTabletDriver, δείτε το [τεύχος ιχνηλάτης](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Όταν Δημιουργία αιτημάτων έλξης, ακολουθήστε τις οδηγίες που περιγράφονται στην [συνεισφορά μας κατευθυντήριες γραμμές](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Αν αντιμετωπίζετε προβλήματα ή προτάσεις, [ανοίξτε ένα πρόβλημα εισιτήριο](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) και συμπληρώστε το πρότυπο με σχετικές πληροφορίες. Χαιρετίζουμε και τα δύο σφάλματα αναφορές, καθώς και νέα tablet για να προσθέσετε υποστήριξη. Σε πολλές περιπτώσεις προσθήκη υποστήριξης Για ένα νέο tablet είναι αρκετά εύκολο.

Για θέματα και δημόσιες σχέσεις που σχετίζονται με την [ιστοσελίδα του OpenTabletDriver](https://opentabletdriver.net), δείτε το αποθετήριο [εδώ](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Υποστήριξη νέας γραφίδας

Αν θέλετε να προσθέσουμε υποστήριξη για ένα νέο tablet, ανοίξτε ένα πρόβλημα ή εγγραφείτε στο [discord](https://discord.gg/9bcMaPkVAR) ζητώντας υποστήριξη. *Εμείς γενικά Προτιμήστε η προσθήκη υποστήριξης για ένα tablet να γίνεται μέσω του Discord, λόγω του Μπρος-πίσω εμπλέκονται*.

Θα σας ζητήσουμε να κάνετε μερικά πράγματα, όπως να κάνετε μια καταγραφή των δεδομένων που αποστέλλονται από το δικό σας Tablet χρησιμοποιώντας το ενσωματωμένο εργαλείο εντοπισμού σφαλμάτων tablet, δοκιμάζοντας χαρακτηριστικά του tablet (κουμπιά συσκευής, κουμπιά στυλό, πίεση στυλό κ.λπ.) με διαφορετικές διαμορφώσεις θα σας στέλνουμε να δοκιμάσετε.

Είστε επίσης ευπρόσδεκτοι να ανοίξετε ένα PR προσθέτοντας υποστήριξη για αυτό μόνοι σας, εάν Έχετε μια καλή αντίληψη για το τι εμπλέκεται.

Γενικά αυτή η διαδικασία είναι σχετικά εύκολη, ειδικά αν πρόκειται για tablet κατασκευαστής για τον οποίο έχουμε ήδη υποστήριξη σε άλλα tablet.
