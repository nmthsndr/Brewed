import { useEffect, useState } from "react";
import {
  Title,
  Table,
  Group,
  Button,
  ActionIcon,
  Text,
  Modal,
  TextInput,
  Textarea,
  Stack,
  LoadingOverlay,
  NumberInput,
  Select,
  Switch,
  Badge,
  Image,
  FileInput,
  SimpleGrid
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { IconEdit, IconTrash, IconPlus, IconUpload, IconX } from "@tabler/icons-react";
import api from "../api/api";
import { notifications } from "@mantine/notifications";

interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  roastLevel: string;
  origin: string;
  isCaffeineFree: boolean;
  isOrganic: boolean;
  imageUrl: string;
  categoryId: number;
  categoryName: string;
}

interface Category {
  id: number;
  name: string;
}

interface ProductFormValues {
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  roastLevel: string;
  origin: string;
  isCaffeineFree: boolean;
  isOrganic: boolean;
  imageUrl: string;
  categoryId: number;
}

const AdminProducts = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [opened, { open, close }] = useDisclosure(false);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [uploadedImageUrl, setUploadedImageUrl] = useState<string>('');

  const form = useForm<ProductFormValues>({
    initialValues: {
      name: '',
      description: '',
      price: 0,
      stockQuantity: 0,
      roastLevel: 'Medium',
      origin: '',
      isCaffeineFree: false,
      isOrganic: false,
      imageUrl: '',
      categoryId: 0
    },
    validate: {
      name: (value) => !value ? 'Product name is required' : null,
      description: (value) => !value ? 'Description is required' : null,
      price: (value) => value <= 0 ? 'Price must be greater than 0' : null,
      stockQuantity: (value) => value < 0 ? 'Stock cannot be negative' : null,
      origin: (value) => !value ? 'Origin is required' : null,
      categoryId: (value) => !value ? 'Category is required' : null
    }
  });

  const loadProducts = async () => {
    try {
      setLoading(true);
      const response = await api.Products.getProducts({ pageSize: 1000 });
      setProducts(response.data.items);
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
    loadProducts();
    loadCategories();
  }, []);

  const handleCreate = () => {
    setModalMode('create');
    form.reset();
    setSelectedFiles([]);
    setUploadedImageUrl('');
    open();
  };

  const handleEdit = (product: Product) => {
    setModalMode('edit');
    setSelectedProduct(product);
    setSelectedFiles([]);
    setUploadedImageUrl(product.imageUrl);
    form.setValues({
      name: product.name,
      description: product.description,
      price: product.price,
      stockQuantity: product.stockQuantity,
      roastLevel: product.roastLevel,
      origin: product.origin,
      isCaffeineFree: product.isCaffeineFree,
      isOrganic: product.isOrganic,
      imageUrl: product.imageUrl,
      categoryId: product.categoryId
    });
    open();
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this product?')) {
      try {
        setLoading(true);
        await api.Products.deleteProduct(id);
        await loadProducts();
        notifications.show({
          title: 'Success',
          message: 'Product deleted successfully',
          color: 'green',
        });
      } catch (error) {
        notifications.show({
          title: 'Error',
          message: 'Failed to delete product',
          color: 'red',
        });
      } finally {
        setLoading(false);
      }
    }
  };

  const handleUploadImages = async () => {
    if (selectedFiles.length === 0) {
      notifications.show({
        title: 'Error',
        message: 'Please select at least one image',
        color: 'red',
      });
      return;
    }

    try {
      setLoading(true);

      if (selectedFiles.length === 1) {
        const response = await api.Files.uploadImage(selectedFiles[0], 'products');
        setUploadedImageUrl(response.data.url);
        form.setFieldValue('imageUrl', response.data.url);
        notifications.show({
          title: 'Success',
          message: 'Image uploaded successfully',
          color: 'green',
        });
      } else {
        const response = await api.Files.uploadMultipleImages(selectedFiles, 'products');
        // Join all uploaded images with semicolon separator
        const joinedUrls = response.data.urls.join(';');
        setUploadedImageUrl(joinedUrls);
        form.setFieldValue('imageUrl', joinedUrls);
        notifications.show({
          title: 'Success',
          message: `${response.data.urls.length} images uploaded successfully`,
          color: 'green',
        });
      }

      setSelectedFiles([]);
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data?.error || 'Failed to upload images',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: ProductFormValues) => {
    // Check if image is uploaded
    if (!uploadedImageUrl && !values.imageUrl) {
      notifications.show({
        title: 'Error',
        message: 'Please upload an image',
        color: 'red',
      });
      return;
    }

    try {
      setLoading(true);

      // Use uploaded image URL or existing image URL
      const productData = {
        ...values,
        imageUrl: uploadedImageUrl || values.imageUrl
      };

      if (modalMode === 'create') {
        await api.Products.createProduct(productData);
        notifications.show({
          title: 'Success',
          message: 'Product created successfully',
          color: 'green',
        });
      } else if (selectedProduct) {
        await api.Products.updateProduct(selectedProduct.id, productData);
        notifications.show({
          title: 'Success',
          message: 'Product updated successfully',
          color: 'green',
        });
      }

      await loadProducts();
      close();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to save product',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '20px', position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      <Group justify="space-between" mb="lg">
        <Title order={2}>Products Management</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={handleCreate}>
          Add Product
        </Button>
      </Group>

      {products.length === 0 ? (
        <Text ta="center" c="dimmed">No products found</Text>
      ) : (
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Image</Table.Th>
              <Table.Th>Name</Table.Th>
              <Table.Th>Category</Table.Th>
              <Table.Th>Price</Table.Th>
              <Table.Th>Stock</Table.Th>
              <Table.Th>Origin</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {products.map((product) => (
              <Table.Tr key={product.id}>
                <Table.Td>
                  <Image
                    src={product.imageUrl}
                    alt={product.name}
                    width={50}
                    height={50}
                    radius="sm"
                  />
                </Table.Td>
                <Table.Td>
                  <Text fw={500}>{product.name}</Text>
                </Table.Td>
                <Table.Td>{product.categoryName}</Table.Td>
                <Table.Td>€{product.price.toFixed(2)}</Table.Td>
                <Table.Td>
                  <Badge color={product.stockQuantity > 10 ? 'green' : product.stockQuantity > 0 ? 'yellow' : 'red'}>
                    {product.stockQuantity}
                  </Badge>
                </Table.Td>
                <Table.Td>{product.origin}</Table.Td>
                <Table.Td>
                  <Group gap={4}>
                    {product.isOrganic && <Badge size="xs" color="green">Organic</Badge>}
                    {product.isCaffeineFree && <Badge size="xs" color="blue">Caffeine Free</Badge>}
                  </Group>
                </Table.Td>
                <Table.Td>
                  <Group gap="xs">
                    <ActionIcon
                      variant="subtle"
                      color="blue"
                      onClick={() => handleEdit(product)}
                    >
                      <IconEdit size={16} />
                    </ActionIcon>
                    <ActionIcon
                      variant="subtle"
                      color="red"
                      onClick={() => handleDelete(product.id)}
                    >
                      <IconTrash size={16} />
                    </ActionIcon>
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      <Modal
        opened={opened}
        onClose={close}
        title={modalMode === 'create' ? 'Add Product' : 'Edit Product'}
        size="lg"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              label="Product Name"
              placeholder="e.g. Espresso"
              required
              {...form.getInputProps('name')}
            />

            <Textarea
              label="Description"
              placeholder="Describe the product..."
              minRows={3}
              required
              {...form.getInputProps('description')}
            />

            <Group grow>
              <NumberInput
                label="Price (€)"
                placeholder="0.00"
                required
                min={0}
                decimalScale={2}
                fixedDecimalScale
                {...form.getInputProps('price')}
              />
              <NumberInput
                label="Stock Quantity"
                placeholder="0"
                required
                min={0}
                {...form.getInputProps('stockQuantity')}
              />
            </Group>

            <Select
              label="Category"
              placeholder="Select category"
              required
              data={categories.map(c => ({ value: c.id.toString(), label: c.name }))}
              value={form.values.categoryId?.toString()}
              onChange={(value) => form.setFieldValue('categoryId', value ? parseInt(value) : 0)}
            />

            <Group grow>
              <Select
                label="Roast Level"
                required
                data={[
                  { value: 'Light', label: 'Light' },
                  { value: 'Light-Medium', label: 'Light-Medium' },
                  { value: 'Medium', label: 'Medium' },
                  { value: 'Medium-Dark', label: 'Medium-Dark' },
                  { value: 'Dark', label: 'Dark' }
                ]}
                {...form.getInputProps('roastLevel')}
              />
              <TextInput
                label="Origin"
                placeholder="e.g. Colombia"
                required
                {...form.getInputProps('origin')}
              />
            </Group>

            <Stack gap="xs">
              <Text size="sm" fw={500}>Product Images</Text>
              <FileInput
                placeholder="Select images"
                multiple
                accept="image/*"
                leftSection={<IconUpload size={16} />}
                value={selectedFiles}
                onChange={setSelectedFiles}
              />
              {selectedFiles.length > 0 && (
                <Group gap="xs">
                  <Button size="xs" onClick={handleUploadImages} leftSection={<IconUpload size={14} />}>
                    Upload {selectedFiles.length} image{selectedFiles.length > 1 ? 's' : ''}
                  </Button>
                  <Button size="xs" variant="outline" color="red" onClick={() => setSelectedFiles([])} leftSection={<IconX size={14} />}>
                    Clear
                  </Button>
                </Group>
              )}
              {uploadedImageUrl && (
                <Stack gap="xs">
                  <Text size="xs" c="dimmed">Uploaded Images ({uploadedImageUrl.split(';').length}):</Text>
                  <SimpleGrid cols={3}>
                    {uploadedImageUrl.split(';').map((url, index) => (
                      <div key={index}>
                        <Image src={url.trim()} alt={`Product ${index + 1}`} height={100} fit="contain" />
                        <Text size="xs" c="dimmed" lineClamp={1}>{url.trim().split('/').pop()}</Text>
                      </div>
                    ))}
                  </SimpleGrid>
                </Stack>
              )}
              {selectedFiles.length > 0 && (
                <SimpleGrid cols={3}>
                  {Array.from(selectedFiles).map((file, index) => (
                    <div key={index}>
                      <Image
                        src={URL.createObjectURL(file)}
                        alt={`Preview ${index + 1}`}
                        height={100}
                        fit="cover"
                      />
                      <Text size="xs" c="dimmed" lineClamp={1}>{file.name}</Text>
                    </div>
                  ))}
                </SimpleGrid>
              )}
            </Stack>

            <Group grow>
              <Switch
                label="Organic"
                {...form.getInputProps('isOrganic', { type: 'checkbox' })}
              />
              <Switch
                label="Caffeine Free"
                {...form.getInputProps('isCaffeineFree', { type: 'checkbox' })}
              />
            </Group>

            <Group justify="flex-end" mt="md">
              <Button variant="outline" onClick={close}>Cancel</Button>
              <Button type="submit">Save</Button>
            </Group>
          </Stack>
        </form>
      </Modal>
    </div>
  );
};

export default AdminProducts;