const state = {
  catalog: null,
  variantByAttackId: {},
};

const elements = {
  attackGrid: document.querySelector("[data-attack-grid]"),
};

const safeText = (value) => (value ? String(value) : "");

const renderAttackCards = (attacks) => {
  elements.attackGrid.innerHTML = "";
  attacks.forEach((attack) => {
    const selectedIndex = state.variantByAttackId[attack.id] ?? 0;
    const selectedVariant = (attack.variants || [])[selectedIndex] || {};

    const card = document.createElement("article");
    card.className = "attack-card";

    card.innerHTML = `
      <div class="attack-card-header">
        <h3 data-attack-name></h3>
        <p class="attack-card-summary" data-attack-summary></p>
      </div>
      <div class="variant-row">
        <div class="variant-label">Variant</div>
        <select class="variant-select" aria-label="Select attack variant"></select>
      </div>
      <div class="variant-details">
        <p data-variant-description></p>
      </div>
    `;

    card.querySelector("[data-attack-name]").textContent = safeText(attack.name);
    card.querySelector("[data-attack-summary]").textContent = safeText(attack.summary);

    const select = card.querySelector(".variant-select");
    (attack.variants || []).forEach((variant, index) => {
      const option = document.createElement("option");
      option.value = String(index);
      option.textContent = safeText(variant.name);
      if (index === selectedIndex) {
        option.selected = true;
      }
      select.appendChild(option);
    });

    const descriptionEl = card.querySelector("[data-variant-description]");
    descriptionEl.textContent = safeText(selectedVariant.description);

    select.addEventListener("change", (event) => {
      const index = Number(event.target.value || 0);
      const variant = (attack.variants || [])[index] || {};
      state.variantByAttackId[attack.id] = index;
      descriptionEl.textContent = safeText(variant.description);
    });

    elements.attackGrid.appendChild(card);
  });
};

const initCatalog = (catalog) => {
  state.catalog = catalog;
  renderAttackCards(state.catalog.attacks || []);
};

const showError = () => {
  elements.attackGrid.innerHTML = "<p class=\"muted-text\">Unable to load attack catalog.</p>";
};

fetch("/data/attacks.json")
  .then((response) => {
    if (!response.ok) {
      throw new Error("Catalog fetch failed");
    }
    return response.json();
  })
  .then(initCatalog)
  .catch(showError);
