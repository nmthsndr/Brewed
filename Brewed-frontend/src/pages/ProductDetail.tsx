import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Title,
  Text,
  Image,
  Group,
  Badge,
  Button,
  Stack,
  Grid,
  Paper,
  LoadingOverlay,
  NumberInput,
  Divider,
  Card,
  Rating,
  Textarea,
  ActionIcon
} from "@mantine/core";
import { IconShoppingCart, IconArrowLeft, IconStar } from "@tabler/icons-react";
import { useForm } from "@mantine/form";
import api, { ReviewCreateDto } from "../api/api";
import { IProduct } from "../interfaces/IProduct";
import { IReview } from "../interfaces/IReview";
import ReviewCard from "../components/ReviewCard";
import { notifications } from "@mantine/notifications";
import useAuth from "../hooks/useAuth";

const ProductDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { userId } = useAuth();
  const [product, setProduct] = useState<IProduct | null>(null);
  const [reviews, setReviews] = useState<IReview[]>([]);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);

  const reviewForm = useForm<ReviewCreateDto>({
    initialValues: {
      productId: 0,
      rating: 5,
      title: '',
      comment: ''
    },
    validate: {
      rating: (val) => (val < 1 || val > 5 ? 'Rating must be between 1 and 5' : null),
      comment: (val) => (val.length < 10 ? 'Comment must be at least 10 characters' : null)
    }
  });

  const loadProduct = async () => {
    try {
      setLoading(true);
      const response = await api.Products.getProduct(parseInt(id!));
      setProduct(response.data);
      reviewForm.setFieldValue('productId', response.data.id);
    } catch (error) {
      console.error("Failed to load product:", error);
      notifications.show({
        title: 'Error',
        message: 'Failed to load product details',
        color: 'red',
      });
    } finally {
      setLoading(false);
    }
  };

  const loadReviews = async () => {
    try {
      const response = await api.Reviews.getProductReviews(parseInt(id!), 1, 10);
      setReviews(response.data.items);
    } catch (error) {
      console.error("Failed to load reviews:", error);
    }
  };

  useEffect(() => {
    if (id) {
      loadProduct();
      loadReviews();
    }
  }, [id]);

  const handleAddToCart = async () => {
    if (!product) return;

    try {
      await api.Cart.addToCart({ productId: product.id, quantity });
      notifications.show({
        title: 'Success',
        message: `${quantity} ${product.name} added to cart`,
        color: 'green',
      });
    } catch (error) {
      notifications.show({
        title: 'Error',
        message: 'Failed to add product to cart',
        color: 'red',
      });
    }
  };

  const handleSubmitReview = async (values: ReviewCreateDto) => {
    try {
      await api.Reviews.createReview(values);
      notifications.show({
        title: 'Success',
        message: 'Review submitted successfully',
        color: 'green',
      });
      reviewForm.reset();
      await loadReviews();
    } catch (error: any) {
      notifications.show({
        title: 'Error',
        message: error.response?.data || 'Failed to submit review',
        color: 'red',
      });
    }
  };

  const handleDeleteReview = async (reviewId: number) => {
    if (window.confirm('Are you sure you want to delete this review?')) {
      try {
        await api.Reviews.deleteReview(reviewId);
        notifications.show({
          title: 'Success',
          message: 'Review deleted successfully',
          color: 'green',
        });
        await loadReviews();
      } catch (error) {
        notifications.show({
          title: 'Error',
          message: 'Failed to delete review',
          color: 'red',
        });
      }
    }
  };

  if (loading || !product) {
    return <LoadingOverlay visible />;
  }

  return (
    <div style={{ padding: '20px' }}>
      <Button
        leftSection={<IconArrowLeft size={16} />}
        variant="subtle"
        onClick={() => navigate('/app/products')}
        mb="lg"
      >
        Back to Products
      </Button>

      <Grid>
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Image
            src={product.imageUrl}
            alt={product.name}
            radius="md"
            height={400}
            fit="cover"
          />
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            <div>
              <Title order={2}>{product.name}</Title>
              <Group mt="xs" gap="xs">
                <Badge color={product.stockQuantity > 0 ? "green" : "red"}>
                  {product.stockQuantity > 0 ? "In Stock" : "Out of Stock"}
                </Badge>
                <Badge variant="light">{product.categoryName}</Badge>
              </Group>
            </div>

            {product.reviewCount > 0 && (
              <Group gap="xs">
                <Rating value={product.averageRating} readOnly fractions={2} />
                <Text size="sm">
                  {product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
                </Text>
              </Group>
            )}

            <Text size="xl" fw={700} c="blue">
            €{product.price.toFixed(2)}
            </Text>

            <Text>{product.description}</Text>

            <Paper withBorder p="md">
              <Stack gap="xs">
                <Group>
                  <Text fw={500}>Roast Level:</Text>
                  <Text>{product.roastLevel}</Text>
                </Group>
                <Group>
                  <Text fw={500}>Origin:</Text>
                  <Text>{product.origin}</Text>
                </Group>
                <Group>
                  <Text fw={500}>Caffeine Free:</Text>
                  <Text>{product.isCaffeineFree ? 'Yes' : 'No'}</Text>
                </Group>
                <Group>
                  <Text fw={500}>Organic:</Text>
                  <Text>{product.isOrganic ? 'Yes' : 'No'}</Text>
                </Group>
              </Stack>
            </Paper>

            {product.stockQuantity > 0 && (
              <Group>
                <NumberInput
                  label="Quantity"
                  value={quantity}
                  onChange={(val) => setQuantity(Number(val))}
                  min={1}
                  max={product.stockQuantity}
                  style={{ width: 100 }}
                />
                <Button
                  leftSection={<IconShoppingCart size={18} />}
                  onClick={handleAddToCart}
                  mt="xl"
                  size="lg"
                >
                  Add to Cart
                </Button>
              </Group>
            )}
          </Stack>
        </Grid.Col>
      </Grid>

      <Divider my="xl" />

      {/* Reviews Section */}
      <Title order={3} mb="md">Customer Reviews</Title>

      <Grid>
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Card withBorder p="lg">
            <Title order={4} mb="md">Write a Review</Title>
            <form onSubmit={reviewForm.onSubmit(handleSubmitReview)}>
              <Stack>
                <div>
                  <Text size="sm" fw={500} mb="xs">Rating</Text>
                  <Rating {...reviewForm.getInputProps('rating')} size="lg" />
                </div>

                <TextInput
                  label="Title (Optional)"
                  placeholder="Summary of your review"
                  {...reviewForm.getInputProps('title')}
                />

                <Textarea
                  label="Review"
                  placeholder="Share your thoughts about this product"
                  required
                  minRows={4}
                  {...reviewForm.getInputProps('comment')}
                />

                <Button type="submit">Submit Review</Button>
              </Stack>
            </form>
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            {reviews.length === 0 ? (
              <Text c="dimmed" ta="center">No reviews yet. Be the first to review!</Text>
            ) : (
              reviews.map((review) => (
                <ReviewCard
                  key={review.id}
                  review={review}
                  canDelete={review.userId.toString() === userId}
                  onDelete={handleDeleteReview}
                />
              ))
            )}
          </Stack>
        </Grid.Col>
      </Grid>
    </div>
  );
};

export default ProductDetail;