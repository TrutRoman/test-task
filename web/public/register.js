// Позначає вимогу галочкою, щойно значення у відповідному полі стає коректним.
// Правила приходять з сервера в data-атрибутах, тому збігаються з серверною валідацією.
const requirements = document.getElementById("requirements");
const form = document.getElementById("register-form");

const emailPattern = new RegExp(requirements.dataset.emailPattern);
const nameMin = Number(requirements.dataset.nameMin);
const nameMax = Number(requirements.dataset.nameMax);
const passwordMin = Number(requirements.dataset.passwordMin);

const rules = {
  email: (value) => emailPattern.test(value.trim()),
  name: (value) => value.trim().length >= nameMin && value.trim().length <= nameMax,
  password: (value) => value.length >= passwordMin,
};

function updateRequirement(field) {
  const input = form.elements.namedItem(field);
  const item = requirements.querySelector(`[data-field="${field}"]`);
  item.classList.toggle("is-valid", rules[field](input.value));
}

for (const field of Object.keys(rules)) {
  form.elements.namedItem(field).addEventListener("input", () => updateRequirement(field));
  updateRequirement(field);
}
