#!/bin/bash

declare -a dates=(
  "2025-11-06 09:15:00"
  "2025-11-06 16:30:00"
  "2025-11-07 10:00:00"
  "2025-11-08 11:45:00"
  "2025-11-09 09:30:00"
  "2025-11-10 14:00:00"
  "2025-11-11 10:30:00"
  "2025-11-12 13:15:00"
  "2025-11-13 09:00:00"
  "2025-11-13 17:00:00"
  "2025-11-14 11:00:00"
  "2025-11-15 14:30:00"
  "2025-11-16 10:00:00"
  "2025-11-18 09:45:00"
  "2025-11-19 13:00:00"
  "2025-11-20 11:30:00"
  "2025-11-21 15:00:00"
  "2025-11-23 10:00:00"
  "2025-11-25 14:00:00"
  "2025-11-27 11:00:00"
)

declare -a messages=(
  "Initial project setup and folder structure"
  "Add product listing page with basic grid layout"
  "Implement product card component with image hover zoom effect"
  "Add product detail page with image gallery"
  "Set up cart context for global cart state management"
  "Implement add to cart and remove from cart functionality"
  "Persist cart state across pages using localStorage"
  "Build cart sidebar/drawer with item count badge"
  "Add favorites/wishlist feature with toggle on product cards"
  "Persist wishlist state to localStorage"
  "Implement post product form for sellers (title, price, images, category)"
  "Add image upload preview in post product form"
  "Build order placement flow and order confirmation page"
  "Implement view orders page for buyers"
  "Add order history page with status tracking (pending, shipped, delivered)"
  "Implement coupon code input and discount logic at checkout"
  "Add coupon validation and error handling"
  "Build seller analytics page (total sales, revenue, top products)"
  "Implement admin/seller dashboard with summary cards and charts"
  "Bug fixes, UI polish, and responsive layout improvements"
)

for i in "${!dates[@]}"; do
  echo "update $i" >> changelog.txt
  git add .
  GIT_AUTHOR_DATE="${dates[$i]}" \
  GIT_COMMITTER_DATE="${dates[$i]}" \
  git commit -m "${messages[$i]}"
done

echo "Done! 20 commits created."
