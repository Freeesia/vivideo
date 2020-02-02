import Vue from "vue";
import VueRouter from "vue-router";
import Home from "@/views/Home.vue";
import Play from "@/views/Play.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/about",
    name: "about",
    component: () => import(/* webpackChunkName: "about" */ "@/views/About.vue")
  },
  {
    path: "/signup/:code",
    name: "signup",
    component: () => import(/* webpackChunkName: "signup" */ "@/views/Signup.vue")
  },
  {
    path: "/invite",
    name: "invite",
    component: () => import(/* webpackChunkName: "invite" */ "@/views/Invite.vue")
  },
  {
    path: "/play/:request*",
    name: "play",
    component: Play
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

export default router;
