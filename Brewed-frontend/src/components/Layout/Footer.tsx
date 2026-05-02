import { ActionIcon, Group, Text } from "@mantine/core";
import {
  IconBrandInstagram,
  IconBrandX,
  IconBrandFacebook,
} from "@tabler/icons-react";
import classes from "./Footer.module.css";


const Footer = () => {

  return (
    <div className={classes.inner}>
      <Text
          fw={500}
          size="sm"
          style={{
            fontFamily: 'serif',
            color: '#8B4513',
            letterSpacing: '-0.02em',
          }}
        >
          © 2026 Brewed
        </Text>

      <Group gap="xs" wrap="nowrap" className={classes.socials}>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Twitter" href="https://x.com" component="a" target="_blank">
          <IconBrandX size={18} stroke={1.5} />
        </ActionIcon>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Facebook" href="https://facebook.com" component="a" target="_blank">
          <IconBrandFacebook size={18} stroke={1.5} />
        </ActionIcon>
        <ActionIcon size="lg" variant="subtle" color="brown" radius="xl" aria-label="Instagram" href="https://instagram.com" component="a" target="_blank">
          <IconBrandInstagram size={18} stroke={1.5} />
        </ActionIcon>
      </Group>
    </div>
  );
};

export default Footer;