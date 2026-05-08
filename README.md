# Recipository

Recipository is a full-stack web application that combines a modern **social recipe community** with an **ingredient marketplace**. The platform enables users to publish recipes, follow creators, curate personal collections, and shop ingredients directly from recipe posts.

## 📌 Project Overview

Recipository delivers a complete cooking and shopping experience in one place:

- **Community-driven recipe publishing** with review and approval workflows.
- **Ingredient marketplace** for purchasing single items or bulk recipe supplies.
- **Order and cart management** including checkout, discounts, and fulfillment tracking.
- **Creator revenue tracking** from purchases attributed to recipes.
- **Nutrition and calorie insights** for recipe content.
- **Search and discovery** across recipes, ingredients, and marketplace items.

## 🚀 Key Features

### Social and Recipe Experience

- Publish and manage recipes.
- Follow creators and view personalized feeds.
- Create, save, and edit recipe collections.
- Review recipes after ordering.
- Recipe approval process for moderation.

### Marketplace and Shopping

- Ingredient browsing and search.
- Add items to cart from recipe ingredient lists.
- Apply discount codes at checkout.
- Track orders through fulfillment stages.

### Creator Rewards

- Revenue attribution for recipe creators.
- Wallet-style transaction history.
- Earnings based on order activity linked to recipe posts.

### Content & Utility

- Per-recipe calorie and nutrition calculations.
- Contextual search with filtering options.
- Rich content interactions like comments and likes.

## 🧱 Architecture

The application is built as a .NET web app with the following structure (MVC):

- `Website/` – main ASP.NET project.
- `Website/Controllers/` – request handlers and API controllers.
- `Website/Data/` – EF Core DB context and application data models.
- `Website/Views/` – Razor pages and UI templates.
- `Website/Services/` – business logic and service interfaces.
- `Website/wwwroot/` – static assets like CSS, JS, and images.

## 🔧 Development Notes

- The application uses Entity Framework Core for data access.
- Razor views are used for server-rendered UI.
- Services are registered through dependency injection.
- Admin functionality includes approval of new recipes and management of marketplace items.

## 📎 Related Links

- [SRS Document](https://docs.google.com/document/d/1KDKBlWZZRyur1LZXwtCXyl6NssXc6m0KX2xjSZnfzLM/)
- [Contribution Tracker](https://docs.google.com/spreadsheets/d/1N9Mg-prmNfoT8Q8t8bWnzrZiZmGxewVTF8TXE7F3Z0A/)
- [Sprint Board](https://github.com/users/imclearlythebest/projects/3) (Abandoned)
