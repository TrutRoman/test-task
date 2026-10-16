// Перемикає світлу/темну тему і запам'ятовує вибір у браузері.
// Початкову тему ставить вбудований скрипт у <head> (див. partials/header.ejs).
document.getElementById("theme-toggle").addEventListener("click", () => {
  const next = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
  document.documentElement.dataset.theme = next;
  localStorage.setItem("theme", next);
});
