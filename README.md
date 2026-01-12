[![Build (Windows)](https://github.com/SAM-BIM/SAM_Revit/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/SAM-BIM/SAM_Revit/actions/workflows/build.yml)
[![Installer (latest)](https://img.shields.io/github/v/release/SAM-BIM/SAM_Deploy?label=installer)](https://github.com/SAM-BIM/SAM_Deploy/releases/latest)

# SAM_Revit

<a href="https://github.com/SAM-BIM/SAM">
  <img src="https://github.com/SAM-BIM/SAM/blob/master/Grasshopper/SAM.Core.Grasshopper/Resources/SAM_Small.png"
       align="left" hspace="10" vspace="6">
</a>

**SAM_Revit** is part of the **SAM (Sustainable Analytical Model) Toolkit** —  
an open-source collection of tools designed to help engineers create, manage,
and process analytical building models for energy and environmental analysis.

This repository provides **integration between Autodesk Revit and SAM analytical models**,
enabling data exchange and workflow coordination between Revit-based BIM models
and SAM’s analytical environment.

The integration leverages **Rhino.Inside.Revit** to connect Revit, Rhino,
and Grasshopper-based SAM workflows within a unified environment.

Welcome — and let’s keep the open-source journey going. 🤝

---

## Requirements

- **Autodesk Revit 2025 or later**
- **Rhino.Inside.Revit**
- **SAM** (installed via the SAM installer)

---

## Installing

To install **SAM**, download and run the  
[latest Windows installer](https://github.com/SAM-BIM/SAM_Deploy/releases/latest).

Once installed, ensure that **Rhino.Inside.Revit** is available
and that you are running a supported version of Revit (2025+).

---

## Documentation

Please refer to the **SAM Wiki** for detailed guidance, setup instructions,
and workflow examples related to Revit integration:

📘 https://github.com/SAM-BIM/SAM/wiki

---

## Resources
- 🧠 **SAM Core:** https://github.com/SAM-BIM/SAM  
- 🦏 **Rhino.Inside.Revit:** https://www.rhino3d.com/inside/revit/  
- 🧰 **SAM Installer:** https://github.com/SAM-BIM/SAM_Deploy/releases/latest  

---

## Development notes

- Target framework: **.NET / C#**
- Integration follows SAM-BIM analytical and BIM interoperability conventions
- New or modified `.cs` files must include the SPDX header from `COPYRIGHT_HEADER.txt`

---

## Licence

This repository is free software licensed under the  
**GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)**.

Each contributor retains copyright to their respective contributions.  
The project history (Git) records authorship and provenance of all changes.

See:
- `LICENSE`
- `NOTICE`
- `COPYRIGHT_HEADER.txt`
