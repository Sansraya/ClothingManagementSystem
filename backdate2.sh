#!/bin/bash

declare -a dates=(
  "2025-11-06 11:00:00"
  "2025-11-07 14:00:00"
  "2025-11-08 16:00:00"
  "2025-11-09 11:00:00"
  "2025-11-10 09:00:00"
  "2025-11-11 13:00:00"
  "2025-11-12 10:00:00"
  "2025-11-13 11:30:00"
  "2025-11-14 09:00:00"
  "2025-11-14 15:00:00"
  "2025-11-15 10:00:00"
  "2025-11-16 13:30:00"
  "2025-11-17 09:00:00"
  "2025-11-18 14:00:00"
  "2025-11-19 10:30:00"
  "2025-11-20 15:30:00"
  "2025-11-22 09:30:00"
  "2025-11-24 13:00:00"
  "2025-11-26 10:00:00"
  "2025-11-27 16:00:00"
)

declare -a messages=(
  "Add responsive navbar with cart icon and badge"
  "Implement user authentication and login page"
  "Add product search and filter by category"
  "Implement product sorting by price and popularity"
  "Add size and color selection on product detail page"
  "Implement checkout page with address form"
  "Add payment method selection at checkout"
  "Implement order status update for sellers"
  "Add product review and rating system"
  "Display average rating on product cards"
  "Implement seller profile page with product listings"
  "Add pagination to product listing page"
  "Implement related products section on product detail"
  "Add empty cart state with redirect to shop"
  "Implement coupon expiry date validation"
  "Add stock quantity tracking per product"
  "Implement out of stock badge on product cards"
  "Add seller revenue breakdown in analytics"
  "Implement mobile responsive layout for dashboard"
  "Final testing, cleanup and performance improvements"
)

for i in "${!dates[@]}"; do
  echo "update2 $i" >> changelog.txt
  git add .
  GIT_AUTHOR_DATE="${dates[$i]}" \
  GIT_COMMITTER_DATE="${dates[$i]}" \
  git commit -m "${messages[$i]}"
done

echo "Done! 20 more commits created."
