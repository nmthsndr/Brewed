import { useEffect, useState } from "react";
import {
  Title,
  Table,
  Group,
  Text,
  Card,
  LoadingOverlay,
  Button,
  Stack,
  Pagination,
  Modal,
  Rating,
  ActionIcon,
  Badge,
  TextInput
} from "@mantine/core";
import { IconStar, IconTrash, IconSearch } from "@tabler/icons-react";
import { useSearchParams } from "react-router-dom";
import api from "../api/api";
import { IReview } from "../interfaces/IReview";
import { notifications } from "@mantine/notifications";

const AdminReviews = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [reviews, setReviews] = useState<IReview[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalPages, setTotalPages] = useState(1);
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState<string>(searchParams.get("search") || "");
  const [appliedSearch, setAppliedSearch] = useState<string>(searchParams.get("search") || "");
  const [selectedReview, setSelectedReview] = useState<IReview | null>(null);
  const [modalOpened, setModalOpened] = useState(false);

  useEffect(() => {
    loadReviews();
  }, [currentPage, appliedSearch]);

  const handleSearch = () => {
    setCurrentPage(1);
    setAppliedSearch(searchQuery);
    if (searchQuery) {
      setSearchParams({ search: searchQuery });
    } else {
      setSearchParams({});
    }
  };

  const loadReviews = async () => {
    try {
      setLoading(true);
      const response = await api.Reviews.getAllReviews(currentPage, 10, appliedSearch || undefined);
      setReviews(response.data.items);
      setTotalPages(response.data.totalPages);
    } catch (error) {
      console.error("Failed to load reviews:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load reviews',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteReview = async (reviewId: number) => {
    if (window.confirm('Are you sure you want to delete this review? This action cannot be undone.')) {
      try {
        await api.Reviews.deleteReview(reviewId);
        notifications.show({
          title: 'Success',
          message: 'Review deleted successfully',
          color: 'green',
        });
        loadReviews();
      } catch (error) {
        console.error("Failed to delete review:", error);
        notifications.show({
          title: 'Error',
          message: 'Failed to delete review',
          color: 'red',
        });
      }
    }
  };

  const openReviewDetails = (review: IReview) => {
    setSelectedReview(review);
    setModalOpened(true);
  };

  const getRatingColor = (rating: number) => {
    if (rating >= 4) return 'green';
    if (rating >= 3) return 'yellow';
    return 'red';
  };

  if (loading && reviews.length === 0) {
    return <LoadingOverlay visible />;
  }

  return (
    <div>
      <Title order={2} mb="xs" style={{ color: '#3d3d3d' }}>All Reviews</Title>
      <Text size="sm" c="dimmed" mb="lg">Monitor and moderate customer reviews</Text>

      <Group mb="lg">
        <TextInput
          placeholder="Search by customer name, email or product"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.currentTarget.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') handleSearch(); }}
          leftSection={<IconSearch size={16} />}
          style={{ flex: 1, maxWidth: 400 }}
        />
        <Button variant="light" onClick={handleSearch}>
          Search
        </Button>
        {appliedSearch && (
          <Button variant="subtle" color="gray" onClick={() => {
            setSearchQuery("");
            setAppliedSearch("");
            setSearchParams({});
            setCurrentPage(1);
          }}>
            Clear
          </Button>
        )}
      </Group>

      {reviews.length === 0 ? (
        <div style={{ padding: '40px', textAlign: 'center' }}>
          <IconStar size={100} color="#ccc" style={{ margin: '0 auto' }} />
          <Title order={3} mt="md" c="dimmed">No reviews found</Title>
          <Text c="dimmed" mt="sm">Customer reviews will appear here</Text>
        </div>
      ) : (
        <>
          <Card withBorder>
            <Table highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Product</Table.Th>
                  <Table.Th>Customer</Table.Th>
                  <Table.Th>Rating</Table.Th>
                  <Table.Th>Title</Table.Th>
                  <Table.Th>Date</Table.Th>
                  <Table.Th>Actions</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {reviews.map((review) => (
                  <Table.Tr key={review.id}>
                    <Table.Td>
                      <Text size="sm" fw={500}>
                        {review.productName || 'Unknown Product'}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm">{review.userName}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Group gap="xs">
                        <Rating value={review.rating} readOnly size="sm" />
                        <Badge size="sm" color={getRatingColor(review.rating)}>
                          {review.rating}/5
                        </Badge>
                      </Group>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm" lineClamp={1}>
                        {review.title}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm">
                        {new Date(review.createdAt).toLocaleDateString()}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <Group gap="xs">
                        <Button
                          size="xs"
                          variant="light"
                          onClick={() => openReviewDetails(review)}
                        >
                          View
                        </Button>
                        <ActionIcon
                          color="red"
                          variant="light"
                          onClick={() => handleDeleteReview(review.id)}
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      </Group>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Card>

          {totalPages > 1 && (
            <Group justify="center" mt="xl">
              <Pagination
                value={currentPage}
                onChange={setCurrentPage}
                total={totalPages}
              />
            </Group>
          )}
        </>
      )}

      <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title="Review Details"
        size="md"
      >
        {selectedReview && (
          <Stack gap="md">
            <div>
              <Text fw={600} size="sm">Product</Text>
              <Text size="sm">{selectedReview.productName || 'Unknown Product'}</Text>
            </div>

            <div>
              <Text fw={600} size="sm">Customer</Text>
              <Text size="sm">{selectedReview.userName}</Text>
            </div>

            <div>
              <Text fw={600} size="sm" mb="xs">Rating</Text>
              <Group gap="xs">
                <Rating value={selectedReview.rating} readOnly />
                <Text size="sm" c="dimmed">({selectedReview.rating}/5)</Text>
              </Group>
            </div>

            <div>
              <Text fw={600} size="sm">Title</Text>
              <Text size="sm">{selectedReview.title}</Text>
            </div>

            <div>
              <Text fw={600} size="sm">Comment</Text>
              <Text size="sm">{selectedReview.comment}</Text>
            </div>

            <div>
              <Text fw={600} size="sm">Date</Text>
              <Text size="sm">
                {new Date(selectedReview.createdAt).toLocaleString()}
              </Text>
            </div>

            <Button
              color="red"
              onClick={() => {
                setModalOpened(false);
                handleDeleteReview(selectedReview.id);
              }}
            >
              Delete Review
            </Button>
          </Stack>
        )}
      </Modal>
    </div>
  );
};

export default AdminReviews;