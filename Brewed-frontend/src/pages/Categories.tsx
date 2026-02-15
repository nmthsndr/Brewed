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
  Badge,
  ScrollArea
} from "@mantine/core";
import { useForm } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { IconEdit, IconTrash, IconPlus, IconSearch } from "@tabler/icons-react";
import api from "../api/api";
import { ICategory } from "../interfaces/ICategory";
import { notifications } from "@mantine/notifications";

const Categories = () => {
  const [categories, setCategories] = useState<ICategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedCategory, setSelectedCategory] = useState<ICategory | null>(null);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [opened, { open, close }] = useDisclosure(false);

  const form = useForm({
    initialValues: {
      name: '',
      description: ''
    },
    validate: {
      name: (value) => !value ? 'Category name is required' : null
    }
  });

  const loadCategories = async () => {
    try {
      setLoading(true);
      const response = await api.Categories.getCategories();
      setCategories(response.data);
    } catch (error) {
      console.error("Failed to load categories:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load categories',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  const handleCreate = () => {
    setModalMode('create');
    form.reset();
    open();
  };

  const handleEdit = (category: ICategory) => {
    setModalMode('edit');
    setSelectedCategory(category);
    form.setValues({
      name: category.name,
      description: category.description
    });
    open();
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this category? This will fail if there are products in this category.')) {
      try {
        setLoading(true);
        await api.Categories.deleteCategory(id);
        await loadCategories();
        notifications.show({
          title: 'Success',
          message: 'Category deleted successfully',
          color: 'green',
        });
      } catch (error: any) {
        notifications.show({
          title: 'Error',
          message: error.response?.data || 'Failed to delete category',
          color: 'red',
        });
      } finally {
        setLoading(false);
      }
    }
  };

  const handleSubmit = async (values: typeof form.values) => {
    try {
      setLoading(true);

      if (modalMode === 'create') {
        await api.Categories.createCategory(values);
        notifications.show({
          title: 'Success',
          message: 'Category created successfully',
          color: 'green',
        });
      } else if (selectedCategory) {
        await api.Categories.updateCategory(selectedCategory.id, values);
        notifications.show({
          title: 'Success',
          message: 'Category updated successfully',
          color: 'green',
        });
      }

      await loadCategories();
      close();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to save category',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ position: 'relative' }}>
      <LoadingOverlay visible={loading} />

      <Group justify="space-between" mb="lg">
        <Title order={2}>Categories Management</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={handleCreate}>
          Add Category
        </Button>
      </Group>

      <Group mb="lg">
        <TextInput
          placeholder="Search by name or description..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.currentTarget.value)}
          leftSection={<IconSearch size={16} />}
          style={{ flex: 1, maxWidth: 400 }}
        />
        {searchQuery && (
          <Button variant="subtle" color="gray" onClick={() => setSearchQuery("")}>
            Clear
          </Button>
        )}
      </Group>

      {(() => {
        const filteredCategories = categories.filter((cat) => {
          if (!searchQuery) return true;
          const q = searchQuery.toLowerCase();
          return cat.name.toLowerCase().includes(q) ||
            cat.description?.toLowerCase().includes(q);
        });
        return filteredCategories.length === 0 ? (
          <Text ta="center" c="dimmed">{searchQuery ? 'No categories match your search' : 'No categories found'}</Text>
        ) : (
        <ScrollArea h="calc(100vh - 260px)">
          <Table striped highlightOnHover stickyHeader>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Name</Table.Th>
                <Table.Th>Description</Table.Th>
                <Table.Th>Products</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {filteredCategories.map((category) => (
                <Table.Tr key={category.id}>
                  <Table.Td>
                    <Text fw={500}>{category.name}</Text>
                  </Table.Td>
                  <Table.Td>{category.description}</Table.Td>
                  <Table.Td>
                    <Badge>{category.productCount} products</Badge>
                  </Table.Td>
                  <Table.Td>
                    <Group gap="xs">
                      <ActionIcon
                        variant="subtle"
                        color="blue"
                        onClick={() => handleEdit(category)}
                      >
                        <IconEdit size={16} />
                      </ActionIcon>
                      <ActionIcon
                        variant="subtle"
                        color="red"
                        onClick={() => handleDelete(category.id)}
                        disabled={category.productCount > 0}
                      >
                        <IconTrash size={16} />
                      </ActionIcon>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </ScrollArea>
        );
      })()}

      <Modal
        opened={opened}
        onClose={close}
        title={modalMode === 'create' ? 'Add Category' : 'Edit Category'}
        size="md"
      >
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <TextInput
              label="Category Name"
              placeholder="e.g. Espresso"
              required
              {...form.getInputProps('name')}
            />

            <Textarea
              label="Description"
              placeholder="Describe this category..."
              minRows={3}
              {...form.getInputProps('description')}
            />

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

export default Categories;