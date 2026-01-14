// Loads attack variants from the portal catalog and wires them into the .NET demo UI.
(() => {
  const DOTNET_ATTACK_MAP = {
    sqli: {
      scenario: "sql-attack",
      typeName: "TaintedInputPlugin.Attacks",
      methodName: "SearchUsers",
    },
    cmdi: {
      scenario: "cmd-attack",
      typeName: "TaintedInputPlugin.Attacks",
      methodName: "ExecuteCommand",
    },
  };

  const normalizeApps = (apps) => (apps || []).map((app) => String(app).toLowerCase());

  const filterDotnetAttacks = (catalog) => {
    const attacks = catalog.attacks || [];
    return attacks.filter((attack) => {
      if (!DOTNET_ATTACK_MAP[attack.id]) {
        return false;
      }
      const apps = normalizeApps(attack.supportedApps);
      return apps.includes("dotnet");
    });
  };

  const fetchCatalog = async () => {
    const response = await fetch("/data/attacks.json", { cache: "no-store" });
    if (!response.ok) {
      throw new Error("Failed to load attack catalog");
    }
    return response.json();
  };

  const formatCompatibility = (value) => {
    if (value === "sqlite") {
      return "SQLite-friendly";
    }
    if (value === "db-specific") {
      return "DB-specific";
    }
    return "";
  };

  const setText = (element, value) => {
    if (element) {
      element.textContent = value || "";
    }
  };

  const initDemoForms = (attacksById) => {
    const forms = document.querySelectorAll("[data-attack-form]");
    if (!forms.length) {
      return;
    }

    forms.forEach((form) => {
      const attackId = form.dataset.attackId;
      const attack = attacksById.get(attackId);
      const select = form.querySelector("[data-attack-variant]");
      const description = form.querySelector("[data-attack-description]");
      const payload = form.querySelector("[data-attack-payload]");
      const compatibility = form.querySelector("[data-attack-compatibility]");
      const parameterInput = form.querySelector("[data-attack-parameter]");
      const submitButton = form.querySelector("button[type=submit]");

      if (!attack || !select || !parameterInput) {
        if (select) {
          select.disabled = true;
        }
        if (submitButton) {
          submitButton.disabled = true;
        }
        setText(description, "Attack catalog entry not found.");
        setText(compatibility, "");
        setText(payload, "");
        return;
      }

      select.innerHTML = "";
      (attack.variants || []).forEach((variant, index) => {
        const option = document.createElement("option");
        option.value = String(index);
        option.textContent = variant.name || `Variant ${index + 1}`;
        select.appendChild(option);
      });

      const updateVariant = () => {
        const index = Number(select.value || 0);
        const variant = (attack.variants || [])[index] || {};
        parameterInput.value = variant.example || "";
        setText(description, variant.description || "");
        setText(payload, variant.example || "");
        setText(compatibility, formatCompatibility(variant.compatibility));
      };

      select.addEventListener("change", updateVariant);
      updateVariant();
    });
  };

  const initExecuteCatalog = (attacks) => {
    const container = document.querySelector("[data-attack-catalog]");
    if (!container) {
      return;
    }

    const typeSelect = container.querySelector("[data-attack-type]");
    const variantSelect = container.querySelector("[data-attack-variant]");
    const description = container.querySelector("[data-attack-description]");
    const payload = container.querySelector("[data-attack-payload]");
    const compatibility = container.querySelector("[data-attack-compatibility]");
    const fillButton = container.querySelector("[data-attack-fill]");

    if (!typeSelect || !variantSelect || !fillButton) {
      return;
    }

    if (!attacks.length) {
      typeSelect.innerHTML = "<option>Catalog unavailable</option>";
      variantSelect.innerHTML = "";
      setText(description, "Unable to load attack catalog from the portal.");
      setText(compatibility, "");
      setText(payload, "");
      fillButton.disabled = true;
      return;
    }

    typeSelect.innerHTML = "";
    attacks.forEach((attack) => {
      const option = document.createElement("option");
      option.value = attack.id;
      option.textContent = attack.name || attack.id;
      typeSelect.appendChild(option);
    });

    const updateVariantOptions = () => {
      const attack = attacks.find((item) => item.id === typeSelect.value) || attacks[0];
      if (!attack) {
        return;
      }

      variantSelect.innerHTML = "";
      (attack.variants || []).forEach((variant, index) => {
        const option = document.createElement("option");
        option.value = String(index);
        option.textContent = variant.name || `Variant ${index + 1}`;
        variantSelect.appendChild(option);
      });

      const updateDetails = () => {
        const index = Number(variantSelect.value || 0);
        const variant = (attack.variants || [])[index] || {};
        setText(description, variant.description || "");
        setText(payload, variant.example || "");
        setText(compatibility, formatCompatibility(variant.compatibility));
      };

      variantSelect.addEventListener("change", updateDetails);
      updateDetails();
    };

    typeSelect.addEventListener("change", updateVariantOptions);
    updateVariantOptions();

    fillButton.addEventListener("click", () => {
      const attack = attacks.find((item) => item.id === typeSelect.value);
      if (!attack) {
        return;
      }
      const mapping = DOTNET_ATTACK_MAP[attack.id];
      const index = Number(variantSelect.value || 0);
      const variant = (attack.variants || [])[index] || {};

      const typeInput = document.getElementById("TypeName");
      const methodInput = document.getElementById("MethodName");
      const parameterInput = document.getElementById("MethodParameter");

      if (typeInput) {
        typeInput.value = mapping.typeName;
      }
      if (methodInput) {
        methodInput.value = mapping.methodName;
      }
      if (parameterInput) {
        parameterInput.value = variant.example || "";
      }
    });
  };

  const init = async () => {
    const catalog = await fetchCatalog();
    const attacks = filterDotnetAttacks(catalog);
    const attacksById = new Map(attacks.map((attack) => [attack.id, attack]));

    initDemoForms(attacksById);
    initExecuteCatalog(attacks);
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => {
      init().catch(() => {
        initDemoForms(new Map());
        initExecuteCatalog([]);
      });
    });
  } else {
    init().catch(() => {
      initDemoForms(new Map());
      initExecuteCatalog([]);
    });
  }
})();
