import Vue from "vue";
import VueRouter, { RouteConfig } from "vue-router";
import Home from "@/views/Home.vue";
import Play from "@/views/Play.vue";
import { AuthModule } from "@/store";

Vue.use(VueRouter);

const routes: RouteConfig[] = [
  {
    path: "/signin",
    name: "signin",
    component: () => import(/* webpackChunkName: "signin" */ "@/views/Signin.vue"),
    meta: {
      anonymous: true
    }
  },
  {
    path: "/signup/:code",
    name: "signup",
    component: () => import(/* webpackChunkName: "signup" */ "@/views/Signup.vue"),
    meta: {
      anonymous: true
    }
  },
  {
    path: "/invite",
    name: "invite",
    component: () => import(/* webpackChunkName: "invite" */ "@/views/Invite.vue")
  },
  {
    path: "/about",
    name: "about",
    component: () => import(/* webpackChunkName: "about" */ "@/views/About.vue")
  },
  {
    path: "/play/",
    name: "play",
    component: Play,
    props: (route: any) => ({
      path: route.query.path
    })
  },
  {
    path: "/:request*",
    name: "home",
    component: Home
  }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});

router.beforeEach(async (to, _, next) => {
  if (to.meta.anonymous) {
    if (await AuthModule.isSignedIn()) {
      next({ name: "home" });
    } else {
      next();
    }
  } else {
    if (await AuthModule.isSignedIn()) {
      next();
    } else {
      next({ name: "signin" });
    }
  }
});

export default router;
