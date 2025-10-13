import { Card, Group, Text, Rating, ActionIcon } from "@mantine/core";
import { IconTrash } from "@tabler/icons-react";
import { IReview } from "../interfaces/IReview";

interface ReviewCardProps {
  review: IReview;
  canDelete?: boolean;
  onDelete?: (reviewId: number) => void;
}

const ReviewCard = ({ review, canDelete, onDelete }: ReviewCardProps) => {
  return (
    <Card withBorder shadow="sm" p="md" radius="md">
      <Group justify="space-between" mb="xs">
        <div>
          <Text fw={500}>{review.userName}</Text>
          <Text size="xs" c="dimmed">
            {new Date(review.createdAt).toLocaleDateString()}
          </Text>
        </div>
        <Group>
          <Rating value={review.rating} readOnly />
          {canDelete && onDelete && (
            <ActionIcon color="red" variant="subtle" onClick={() => onDelete(review.id)}>
              <IconTrash size={18} />
            </ActionIcon>
          )}
        </Group>
      </Group>

      {review.title && (
        <Text fw={500} mb="xs">
          {review.title}
        </Text>
      )}

      <Text size="sm">{review.comment}</Text>
    </Card>
  );
};

export default ReviewCard;