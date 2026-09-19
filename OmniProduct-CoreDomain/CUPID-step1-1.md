### Step 1.1: Single Responsibility → Unix Philosophy
**"One class, one reason to change"**

**Problem**: `ProductService` handles catalog, pricing, stock, supplier notification, and transport — all at once.

**Plan**:
- Identify every distinct concern currently living in `ProductService` and `Product`
- Extract each concern into its own class with a single focus
- Use the domain vocabulary to name each class (not `ProductCatalogManager` — just `Catalog`)



** Exercise **

extract a Product smallest entity that fits the Warehouse BC

keep only properties that are usefull for stocking the merchandise in a Warehouse
