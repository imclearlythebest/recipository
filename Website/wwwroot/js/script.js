// Find and store header height for CSS usage
const header = document.querySelector('header');
document.documentElement.style.setProperty(
    '--header-height',
    header.offsetHeight + 'px'
);