import { useEffect, useState } from "react";
import {
  Title,
  Grid,
  Group,
  TextInput,
  Select,
  Button,
  LoadingOverlay,
  Pagination,
  Paper,
  Stack,
  MultiSelect,
  NumberInput,
  Checkbox
} from "@mantine/core";
import { IconSearch, IconFilter } from "@tabler/icons-react";
import { useDebouncedValue } from "@mantine/hooks";
import api, { ProductFilterDto } from "../api/api";
import { IProduct } from "../interfaces/IProduct";
import { ICategory } from "../interfaces/ICategory";
import ProductCard from "../components/ProductCard";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";
import useCart from "../hooks/useCart";
import { getGuestSessionId } from "../utils/guestSession";

const Products = () => {
  const [products, setProducts] = useState<IProduct[]>([]);
  const [categories, setCategories] = useState<ICategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearch] = useDebouncedValue(searchQuery, 500);
  const { isLoggedIn } = useAuth();
  const { refreshCartCount } = useCart();

  const [filters, setFilters] = useState<ProductFilterDto>({
    page: 1,
    pageSize: 12,
    sortBy: 'name'
  });

  const loadProducts = async () => {
    try {
      setLoading(true);
      const response = await api.Products.getProducts({
        ...filters,
        search: debouncedSearch || undefined,
        page
      });
      setProducts(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error("Failed to load products:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load products',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await api.Categories.getCategories();
      setCategories(response.data);
    } catch (error) {
      console.error("Failed to load categories:", error);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  useEffect(() => {
    loadProducts();
  }, [page, debouncedSearch, filters]);

  const handleAddToCart = async (productId: number) => {
    try {
      const product = products.find(p => p.id === productId);
      const sessionId = isLoggedIn ? undefined : getGuestSessionId();
      await api.Cart.addToCart({ productId, quantity: 1 }, sessionId);
      await refreshCartCount();
      notifications.show({
        title: 'Success',
        message: product ? `1 ${product.name} added to cart` : 'Product added to cart',
        color: 'green',
      });
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to add product to cart',
        color: 'red',
      });
    }
  };

  const resetFilters = () => {
    setFilters({
      page: 1,
      pageSize: 12,
      sortBy: 'name'
    });
    setSearchQuery('');
    setPage(1);
  };

  // Check if selected category is coffee beans
  const selectedCategory = categories.find(c => c.id === filters.categoryId);
  const isCoffeeBeanCategory = selectedCategory?.name.toLowerCase().includes('bean') ||
                                selectedCategory?.name.toLowerCase().includes('coffee bean') ||
                                selectedCategory?.name.toLowerCase().includes('kávébab');

  return (
    <div style={{ padding: '20px', position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      <Title order={2} mb="lg">Products</Title>

      {/* Filters */}
      <Paper withBorder p="md" mb="lg">
        <Stack gap="md">
          <TextInput
            placeholder="Search products..."
            leftSection={<IconSearch size={16} />}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />

          <Grid>
            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <Select
                label="Category"
                placeholder="All categories"
                clearable
                data={categories.map(c => ({ value: c.id.toString(), label: c.name }))}
                value={filters.categoryId?.toString() || null}
                onChange={(value) => {
                  const newFilters = {
                    ...filters,
                    categoryId: value ? parseInt(value) : undefined
                  };
                  // Clear roast level, organic, and caffeine-free if not coffee beans
                  const isCoffeeBean = value && categories.find(c => c.id === parseInt(value))?.name.toLowerCase().includes('bean');
                  if (!isCoffeeBean) {
                    newFilters.roastLevel = undefined;
                    newFilters.isOrganic = undefined;
                    newFilters.isCaffeineFree = undefined;
                  }
                  setFilters(newFilters);
                }}
              />
            </Grid.Col>

            {isCoffeeBeanCategory && (
              <>
                <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
                  <Select
                    label="Roast Level"
                    placeholder="Any"
                    clearable
                    data={[
                      { value: 'Light', label: 'Light' },
                      { value: 'Light-Medium', label: 'Light-Medium' },
                      { value: 'Medium', label: 'Medium' },
                      { value: 'Medium-Dark', label: 'Medium-Dark' },
                      { value: 'Dark', label: 'Dark' }
                    ]}
                    value={filters.roastLevel || null}
                    onChange={(value) => setFilters({ ...filters, roastLevel: value || undefined })}
                  />
                </Grid.Col>

                <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
                  <Stack gap="xs">
                    <Checkbox
                      label="Organic"
                      checked={filters.isOrganic || false}
                      onChange={(e) => setFilters({ ...filters, isOrganic: e.currentTarget.checked ? true : undefined })}
                    />
                    <Checkbox
                      label="Caffeine Free"
                      checked={filters.isCaffeineFree || false}
                      onChange={(e) => setFilters({ ...filters, isCaffeineFree: e.currentTarget.checked ? true : undefined })}
                    />
                  </Stack>
                </Grid.Col>
              </>
            )}

            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <Select
                label="Sort By"
                data={[
                  { value: 'name', label: 'Name' },
                  { value: 'price-asc', label: 'Price: Low to High' },
                  { value: 'price-desc', label: 'Price: High to Low' },
                  { value: 'rating', label: 'Rating' }
                ]}
                value={filters.sortBy}
                onChange={(value) => setFilters({ ...filters, sortBy: value || 'name' })}
              />
            </Grid.Col>

            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <Group mt="xl">
                <Button variant="outline" onClick={resetFilters}>
                  Reset Filters
                </Button>
              </Group>
            </Grid.Col>
          </Grid>
        </Stack>
      </Paper>

      {/* Products Grid */}
      <Grid>
        {products.length === 0 ? (
          <Grid.Col span={12}>
            <Paper p="xl" ta="center">
              <Title order={4} c="dimmed">No products found</Title>
            </Paper>
          </Grid.Col>
        ) : (
          products.map((product) => (
            <Grid.Col key={product.id} span={{ base: 12, sm: 6, md: 4, lg: 3 }}>
              <ProductCard product={product} onAddToCart={handleAddToCart} />
            </Grid.Col>
          ))
        )}
      </Grid>

      {/* Pagination */}
      {totalPages > 1 && (
        <Group justify="center" mt="xl">
          <Pagination
            total={totalPages}
            value={page}
            onChange={setPage}
          />
        </Group>
      )}
    </div>
  );
};

export default Products;